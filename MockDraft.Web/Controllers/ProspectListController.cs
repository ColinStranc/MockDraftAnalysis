using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockDraft.Web.Models;

namespace MockDraft.Web.Controllers
{
    public class ProspectListController : Controller
    {
        // GET: ProspectList
        public JsonResult List(int count)
        {
            var prospects = new List<Prospect>();
            Prospect brown = new Prospect()
            {
                Name = "Logan Brown",
                Team = "Windsor Spitfires"
            };
            prospects.Add(brown);

            Prospect tkachuk = new Prospect()
            {
                Name = "Matthew Tkachuk",
                Team = "London Knights"
            };
            prospects.Add(tkachuk);

            return new JsonResult() { 
                Data = prospects,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };
        }
    }
}