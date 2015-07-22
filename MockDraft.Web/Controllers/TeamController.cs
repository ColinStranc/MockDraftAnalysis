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
            var teamModel = new CreateTeamModel();
            return View(teamModel);
        }

        [HttpPost]
        public ActionResult Create(CreateTeamModel createTeamModel)
        {
            var teamModel = createTeamModel.TeamModel;
            teamModel.League = GetLeagueWithId(createTeamModel.LeagueId, createTeamModel.PossibleLeagues);
            var dTeam = Mapper.Map<DTeam>(teamModel);
            IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

            if (ModelState.IsValid)
            {
                if (db.TeamExists(dTeam))
                {
                    ViewBag.Feedback = createTeamModel.AlreadyExistedErrorMessage;
                    return View(teamModel);
                }

                db.AddTeam(dTeam);
                ViewBag.Feedback = createTeamModel.SuccessMessage;
            }

            var newTeamModel = new CreateTeamModel();
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