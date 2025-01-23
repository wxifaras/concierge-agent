namespace concierge_agent_api.Models;

public class AttendanceReport
{
    public string section_name { get; set; }
    public string row_name { get; set; }
    public string channel_ind { get; set; }
    public string device_type { get; set; }
    public long seat_num { get; set; }
    public string gate { get; set; }
    public long acct_id { get; set; }
    public string scan_type { get; set; }
    public string event_name { get; set; }
    public DateTime dateOnly { get; set; }
    public DateTime action_time { get; set; }
}
