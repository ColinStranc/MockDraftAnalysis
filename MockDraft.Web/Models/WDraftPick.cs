namespace MockDraft.Web.Models
{
    public class WDraftPick
    {
        public int Id { get; set; }
        public WDraft Draft { get; set; }
        public WProspect Prospect { get; set; }
        public WTeam Team { get; set; }
        public int PickNumber { get; set; }
    }
}