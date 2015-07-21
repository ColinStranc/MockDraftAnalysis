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
    public class TeamController : Controller
    {
        [HttpGet]
        public ActionResult Create()
        {
            var teamModel = new TeamWithPossibleLeagues();
            return View(teamModel);
        }

        [HttpPost]
        public ActionResult Create(TeamWithPossibleLeagues teamModel)
        {
            teamModel.League = GetLeagueWithId(teamModel.LeagueId, teamModel.PossibleLeagues);
            var dTeam = Mapper.Map<DTeam>((WTeam)teamModel);
            IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

            if (ModelState.IsValid)
            {
                if (db.TeamExists(dTeam))
                {
                    ViewBag.Feedback = "Team " + teamModel.Name + " already exists.";
                    return View(teamModel);
                }
                ViewBag.Feedback = "";

                db.AddTeam(dTeam);
            }

            ViewBag.Feedback = "Team " + teamModel.Name + " successfully created.";
            var newTeamModel = new TeamWithPossibleLeagues();
            return View(newTeamModel);
        }

        public WLeague GetLeagueWithId(int id, List<WLeague> leagues)
        {
            foreach (var league in leagues)
            {
                if (league.Id == id) return league;
            }

            return null;
        }
    }
}