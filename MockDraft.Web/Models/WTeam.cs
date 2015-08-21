using System.ComponentModel.DataAnnotations;

namespace MockDraft.Web.Models
{
    public class WTeam
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public WLeague League { get; set; }
    }
}