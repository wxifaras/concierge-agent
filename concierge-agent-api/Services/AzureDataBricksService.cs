using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Section = concierge_agent_api.Models.Section;

namespace concierge_agent_api.Services;

public interface IAzureDatabricksService
{
    Task<Customer> GetCustomerByEmailAsync(string emailAddress);
    Task<Customer> GetCustomerBySmsNumberAsync(string smsNumber);
    Task<DimEventMaster> GetEventMasterAsync(string eventId);
    Task<LotLocation> GetLotLocationsAsync(bool isLot);
    Task<LotLookup> GetLotLookupAsync(string actualLot);
    Task<Section> GetSectionAsync(string sectionId);
    Task<FutureTicket> GetFutureTicketAsync(string email, string tmEventId);
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

    public async Task<Customer> GetCustomerByEmailAsync(string emailAddress)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.ambse.customer WHERE TMEmail = '{emailAddress}'";
        var jsonString = await GetAsync(query);
        var customer = JsonConvert.DeserializeObject<Customer>(jsonString);
        return customer;
    }

    public async Task<Customer> GetCustomerBySmsNumberAsync(string smsNumber)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.ambse.customer WHERE EpsilonSMSNumber = '{smsNumber}'";
        var jsonString = await GetAsync(query);
        var customer = JsonConvert.DeserializeObject<Customer>(jsonString);
        return customer;
    }

    public async Task<DimEventMaster> GetEventMasterAsync(string eventId)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_silver_catalog.event.dimeventmaster WHERE TMEventId = '{eventId}'";
        var jsonString = await GetAsync(query);
        var eventMaster = JsonConvert.DeserializeObject<DimEventMaster>(jsonString);
        return eventMaster;
    }

    public async Task<LotLocation> GetLotLocationsAsync(bool isLot)
    {
        var locationType = isLot ? "Lot" : "Gate";

        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.parking.lot_location WHERE locationType = '{locationType}'";
        var jsonString = await GetAsync(query);
        var lotLocation = JsonConvert.DeserializeObject<LotLocation>(jsonString);
        return lotLocation;
    }

    public async Task<LotLookup> GetLotLookupAsync(string actualLot)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.parking.lot_lookup WHERE actual_lot = '{actualLot}'";
        var jsonString = await GetAsync(query);
        var lotLookup = JsonConvert.DeserializeObject<LotLookup>(jsonString);
        return lotLookup;
    }

    /// <summary>
    /// Get Attendance Report for a customer
    /// </summary>
    /// <param name="tmAcctId">TMAcctId</param>
    /// <returns></returns>
    public async Task<AttendanceReport> GetAttendanceReportAsync(string tmAcctId)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.tm.attendance_report WHERE acct_id = '{tmAcctId}'";
        var jsonString = await GetAsync(query);
        var attendanceReport = JsonConvert.DeserializeObject<AttendanceReport>(jsonString);
        return attendanceReport;
    }

    public async Task<Section> GetSectionAsync(string sectionId)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_silver_catalog.stadium.section WHERE SectionId = '{sectionId}'";
        var jsonString = await GetAsync(query);
        var section = JsonConvert.DeserializeObject<Section>(jsonString);
        return section;
    }

    public async Task<FutureTicket> GetFutureTicketAsync(string email, string tmEventId)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_silver_catalog.ticket.futureticket WHERE emailaddress = '{email}' AND TMEventId = '{tmEventId}'";
        var jsonString = await GetAsync(query);
        var futureTicket = JsonConvert.DeserializeObject<FutureTicket>(jsonString);
        return futureTicket;
    }

    private async Task<string> GetAsync(string query)
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

    private static async Task<string> PollQueryStatusAsync(string statementId)
    {
        while (true)
        {
            var statusResponse = await _client.GetAsync($"/api/2.0/sql/statements/{statementId}");

            if (statusResponse.IsSuccessStatusCode)
            {
                var statusResult = await statusResponse.Content.ReadAsStringAsync();
                var statusJson = JObject.Parse(statusResult);
                JArray dataArray = (JArray)statusJson["result"]["data_array"];
                var state = statusJson["status"]["state"].ToString();

                if (state == "SUCCEEDED")
                {
                    return dataArray[0][0].ToString().ToString();
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