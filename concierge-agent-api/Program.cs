using Asp.Versioning;
using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using concierge_agent_api.Plugins;
using concierge_agent_api.Prompts;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Load configuration from appsettings.json and appsettings.local.json
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddMvc() // This is needed for controllers
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOptions<AzureOpenAiOptions>()
           .Bind(builder.Configuration.GetSection(AzureOpenAiOptions.AzureOpenAI))
           .ValidateDataAnnotations();

builder.Services.AddOptions<CosmosDbOptions>()
           .Bind(builder.Configuration.GetSection(CosmosDbOptions.CosmosDb))
           .ValidateDataAnnotations();

builder.Services.AddOptions<DatabricksOptions>()
           .Bind(builder.Configuration.GetSection(DatabricksOptions.AzureDatabricks))
           .ValidateDataAnnotations();

builder.Services.AddOptions<AzureMapsOptions>()
           .Bind(builder.Configuration.GetSection(AzureMapsOptions.AzureMaps))
           .ValidateDataAnnotations();

builder.Services.AddOptions<AzureStorageOptions>()
           .Bind(builder.Configuration.GetSection(AzureStorageOptions.AzureStorage))
           .ValidateDataAnnotations();

// Build the service provider
var serviceProvider = builder.Services.BuildServiceProvider();

var kernelOptions = serviceProvider.GetRequiredService<IOptions<AzureOpenAiOptions>>().Value;
var databricksOptions = serviceProvider.GetRequiredService<IOptions<DatabricksOptions>>();

builder.Services.AddSingleton<IAzureDatabricksService>(sp =>
{
    var databricksOptions = sp.GetRequiredService<IOptions<DatabricksOptions>>();
    return new AzureDatabricksService(databricksOptions);
});

var azureMapsOptions = serviceProvider.GetRequiredService<IOptions<AzureMapsOptions>>();
IAzureMapsService azureMapsService = new AzureMapsService(azureMapsOptions);

builder.Services.AddMemoryCache();

builder.Services.AddTransient<Kernel>(s =>
{
    var builder = Kernel.CreateBuilder();
    builder.AddAzureOpenAIChatCompletion(kernelOptions.DeploymentName, kernelOptions.EndPoint, kernelOptions.ApiKey);
    var directionsPluginLogger = s.GetRequiredService<ILogger<DirectionsPlugin>>();
    var memoryCache = s.GetRequiredService<IMemoryCache>();

    var azureDatabricksService = s.GetRequiredService<IAzureDatabricksService>();

    var directionsPlugin = new DirectionsPlugin(directionsPluginLogger, azureDatabricksService, azureMapsService, memoryCache);
    builder.Plugins.AddFromObject(directionsPlugin, "DirectionsPlugin");

    var eventsPluginLogger = s.GetRequiredService<ILogger<EventsPlugin>>();
    var eventsPlugin = new EventsPlugin(eventsPluginLogger, serviceProvider.GetRequiredService<IOptions<AzureStorageOptions>>());
    builder.Plugins.AddFromObject(eventsPlugin, "EventsPlugin");
    return builder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
         sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

builder.Services.AddSingleton<IChatHistoryManager>(sp =>
{
    var sysPrompt = CorePrompts.GetSystemPrompt();
    return new ChatHistoryManager(sysPrompt);
});

//var cosmosDbOptions = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>();
//builder.Services.AddSingleton<ISKCosmosDbChatManager>(sp =>
//{
//    var sysPrompt = CorePrompts.GetSystemPrompt();
//    return new SKCosmosDbChatManager(cosmosDbOptions, sysPrompt);
//});

builder.Services.AddHostedService<SmsQueueProcessor>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IAzureDatabricksService, AzureDatabricksService>();
builder.Services.AddSingleton<IAzureMapsService, AzureMapsService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();