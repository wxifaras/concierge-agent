using Azure.Identity;
using concierge_agent_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace concierge_agent_api.Services;

public interface ICosmosDbService
{

}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(IOptions<CosmosDbOptions> options)
    {
        CosmosClient cosmosClient = new(
           accountEndpoint: options.Value.AccountUri,
           tokenCredential: new DefaultAzureCredential(
               new DefaultAzureCredentialOptions
               {
                   TenantId = options.Value.TenantId,
                   ExcludeEnvironmentCredential = true
               })
       );

       _container = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.ContainerName);
    }
}