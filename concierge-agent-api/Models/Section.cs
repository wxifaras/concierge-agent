namespace concierge_agent_api.Models
{
    public class Section
    {
        public long SectionId { get; set; }
        public string SectionName { get; set; }
        public long Priority { get; set; }
        public long SectionClassId { get; set; }
        public long SectionClassGroupId { get; set; }
        public long ConcourseId { get; set; }
        public long SideOfFieldId { get; set; }
        public string lookup_name { get; set; }
        public string PerCapCategoryId { get; set; }
        public string SectionClassGroup { get; set; }
        public string SectionClass { get; set; }
        public long DashboardSort { get; set; }
        public bool Reviewed { get; set; }
        public DateTime ETL_Create_DTM { get; set; }
        public string ETL_Update_DTM { get; set; }
    }
}
