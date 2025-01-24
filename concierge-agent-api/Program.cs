using Asp.Versioning;
using concierge_agent_api.Models;
using concierge_agent_api.Services;

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

builder.Services.AddOptions<AzureStorageOptions>()
           .Bind(builder.Configuration.GetSection(AzureStorageOptions.AzureStorage))
           .ValidateDataAnnotations();

builder.Services.AddHostedService<SmsQueueProcessor>();

builder.Services.AddOptions<AzureMapsOptions>()
              .Bind(builder.Configuration.GetSection(AzureMapsOptions.AzureMaps))
              .ValidateDataAnnotations();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IAzureDatabricksService, AzureDatabricksService>();
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
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