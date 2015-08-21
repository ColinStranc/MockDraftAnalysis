namespace DatabaseModels
{
    public class DDraftPick
    {
        public int Id { get; set; }
        public DDraft Draft { get; set; }
        public DProspect Prospect { get; set; }
        public DTeam Team { get; set; }
        public int PickNumber { get; set; }
    }
}
