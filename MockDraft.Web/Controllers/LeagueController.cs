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
        [HttpGet]
        public ActionResult Create()
        {
            var league = new WLeague();
            return View(league);
        }

        [HttpPost]
        public ActionResult Create(WLeague league)
        {
            var dLeague = Mapper.Map<DLeague>(league);
            IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

            if (ModelState.IsValid)
            {
                if (db.LeagueNameExists(league.Name))
                {
                    ViewBag.Feedback = "League " + league.Name + " already exists.";
                    return View(league);
                }
                ViewBag.Feedback = "";

                db.AddLeague(dLeague);
                ViewBag.Feedback = "" + league.Name + " successfully created.";
            }

            return View();
        }
    }
}