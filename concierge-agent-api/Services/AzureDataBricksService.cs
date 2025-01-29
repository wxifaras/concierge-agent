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
    Task<List<LotLocation>> GetLotLocationsAsync(bool isLot, string tmEventId);
    Task<List<LotLookup>> GetLotLookupAsync();
    Task<List<LotEventPrice>> GetLotPriceByTMEventIdAsync(string tmEventId);
    Task<Section> GetSectionAsync(string sectionId);
    Task<FutureTicket> GetFutureTicketAsync(string email, string tmEventId);
}

public class AzureDatabricksService : IAzureDatabricksService
{
    private static readonly HttpClient _client;
    private readonly string _databricksInstance;
    private readonly string _databricksToken;
    private readonly string _warehouseId;

    private const string BASE_CUSTOMER_QUERY = "SELECT STRUCT(TMEmail, FirstName, LastName, TMAcctId, EpsilonCustomerKey, WicketId, ConstellationId, AifiCustomerId, CASE WHEN EpsilonSMSNumber IS NOT NULL AND EpsilonSMSNumber != 'null' THEN EpsilonSMSNumber WHEN TMCellPhone IS NOT NULL AND TMCellPhone != 'null' THEN TMCellPhone WHEN TMMAPhone IS NOT NULL AND TMMAPhone != 'null' THEN TMMAPhone ELSE NULL END AS PreferredPhoneNumber, hasBioPhoto, bioWebOnboarded, bioAppOnboarded, bioDateJoined, bioLastUpdated, geniusCheckoutStoredCards, storedCards, signUpMethod, CurrentFalconsSTM, CurrentUnitedSTM, emailOptIn_AF, emailOptIn_AU, emailOptIn_MBS, smsFalconsOptInFlag, smsUniteOptInFlag, smsMBSOptInFlag) FROM ambse_prod_gold_catalog.ambse.customer";

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
        var query = $"{BASE_CUSTOMER_QUERY} WHERE TMEmail = '{emailAddress}'";
        var jsonString = await GetAsync(query);
        var customer = JsonConvert.DeserializeObject<Customer>(jsonString);
        return customer;
    }

    public async Task<Customer> GetCustomerBySmsNumberAsync(string smsNumber)
    {
        var query = $"{BASE_CUSTOMER_QUERY} WHERE EpsilonSMSNumber = '{smsNumber}' OR TMCellPhone = '{smsNumber}' OR TMMAPhone = '{smsNumber}'";
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

    public async Task<List<LotLocation>> GetLotLocationsAsync(bool isLot, string tmEventId)
    {
        var locationType = isLot ? "Lot" : "Gate";

        var query =
            "SELECT " +
            "STRUCT(" +
            "  lot_location.actual_lot, " +
            "  lot_location.lat, " +
            "  lot_location.long, " +
            "  lot_location.locationType, " +
            "  event_lot_cost.lot_price, " +
            "  event_lot_cost.TMEventId) AS combined_struct " +
            "FROM " +
            "ambse_prod_gold_catalog.parking.lot_location AS lot_location " +
            "JOIN " +
            "ambse_prod_gold_catalog.parking.event_lot_cost AS event_lot_cost " +
            "ON " +
            "lot_location.actual_lot = event_lot_cost.actual_lot " +
            "WHERE " +
            "lot_location.locationType = 'Lot' " +
            "AND event_lot_cost.TMEventId = '" + tmEventId + "'";

        var jsonString = await GetAsync(query);
        var lotLocations = JsonConvert.DeserializeObject<List<LotLocation>>(jsonString);
        return lotLocations;
    }

    public async Task<List<LotLookup>> GetLotLookupAsync()
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.parking.lot_lookup";
        var jsonString = await GetAsync(query);
        var lotLookup = JsonConvert.DeserializeObject<List<LotLookup>>(jsonString);
        return lotLookup;
    }
    public async Task<List<LotEventPrice>> GetLotPriceByTMEventIdAsync(string tmEventId)
    {
        var query = $"SELECT STRUCT(*) FROM ambse_prod_gold_catalog.parking.event_lot_cost WHERE TMEventId = '{tmEventId}'";
        var jsonString = await GetAsync(query);
        var lotPrice = JsonConvert.DeserializeObject<List<LotEventPrice>>(jsonString);
        return lotPrice;
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

                var state = statusJson["status"]["state"].ToString();

                if (state == "SUCCEEDED")
                {
                    JArray dataArray = (JArray)statusJson["result"]["data_array"];

                    // Check if there is only one row, and if so, return it directly
                    if (dataArray.Count == 1)
                    {
                        var parsedRow = JObject.Parse(dataArray[0][0].ToString());
                        return parsedRow.ToString();
                    }

                    // If there are multiple rows, parse them and return as an array
                    JArray allData = new JArray();
                    foreach (var row in dataArray)
                    {
                        var parsedRow = JObject.Parse(row[0].ToString());
                        allData.Add(parsedRow);
                    }

                    return allData.ToString();
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