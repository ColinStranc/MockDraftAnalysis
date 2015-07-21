using AutoMapper;
using Database;
using DatabaseModels;
using MockDraft.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MockDraft.Web.Controllers
{
    public class LeagueController : Controller
    {
        // GET: League
        [HttpGet]
        public ActionResult Create()
        {
            var league = new WLeague();
            return View(league);
        }

        [HttpPost]
        public ActionResult Create(WLeague league)
        {
            if (ModelState.IsValid)
            {
                IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

                db.AddLeague(Mapper.Map<DLeague>(league));
            }

            return View();
        }
    }
}