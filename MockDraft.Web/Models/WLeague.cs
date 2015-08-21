using System.ComponentModel.DataAnnotations;

namespace MockDraft.Web.Models
{
    public class WLeague
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}