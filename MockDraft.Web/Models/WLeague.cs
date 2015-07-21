using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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