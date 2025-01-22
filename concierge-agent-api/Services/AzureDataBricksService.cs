using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace concierge_agent_api.Services
{
    public interface IAzureDatabricksService
    {
        Task<string> GetAsync(string query);
        Task<string> DescribeTableAsync(string tableName);
    }

    public class AzureDatabricksService : IAzureDatabricksService
    {
        private static readonly HttpClient _client;
        private readonly string _databricksInstance;
        private readonly string _databricksToken;
        private readonly string _warehouseId;

        static AzureDatabricksService()
        {
            _client = new HttpClient();
        }

        public AzureDatabricksService(IOptions<DatabricksOptions> options)
        {
            _databricksInstance = options.Value.DatabricksInstance;
            _databricksToken = options.Value.Token;
            _warehouseId = options.Value.WarehouseId;

            _client.BaseAddress = new Uri(_databricksInstance);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _databricksToken);
        }

        public async Task<string> GetAsync(string query)
        {
            var content = new StringContent($"{{\"warehouse_id\":\"{_warehouseId}\",\"statement\":\"{query}\",\"wait_timeout\":\"0s\"}}", System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/2.0/sql/statements", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(result);

                string statementId = jsonResponse["statement_id"].ToString();

                var queryResult = await PollQueryStatusAsync(statementId);
                return queryResult;
            }
            else
            {
                throw new Exception($"Error querying table: {response.ReasonPhrase}");
            }
        }
        
        public async Task<string> DescribeTableAsync(string tableName)
        {
            var query = $"DESCRIBE TABLE {tableName}";
            var result = await GetAsync(query);
            return result;
        }

        private static async Task<string> PollQueryStatusAsync(string statementId)
        {
            while (true)
            {
                var statusResponse = await _client.GetAsync($"/api/2.0/sql/statements/{statementId}");

                if (statusResponse.IsSuccessStatusCode)
                {
                    var statusResult = await statusResponse.Content.ReadAsStringAsync();
                    var statusJson = JObject.Parse(statusResult);
                    var state = statusJson["status"]["state"].ToString();

                    if (state == "SUCCEEDED")
                    {
                        return statusJson["result"].ToString();
                    }
                    else if (state == "FAILED")
                    {
                        throw new Exception("Query failed.");
                    }
                }
                else
                {
                    throw new Exception($"Error checking query status: {statusResponse.ReasonPhrase}");
                }

                // Wait for a short period before polling again
                await Task.Delay(2000);
            }
        }
    }
}
