using System;

namespace MockDraft.Web.Models
{
    public class WDraft
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int DraftYear { get; set; }
        public int DraftLength { get; set; }
        public string DraftType { get; set; }
    }
}