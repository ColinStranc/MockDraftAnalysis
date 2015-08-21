using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using Database;
using DatabaseModels;
using MockDraft.Web.Models;

namespace MockDraft.Web.Controllers
{
    public class ProspectController : Controller
    {
        public ActionResult ListHome()
        {
            return View();
        }

        public JsonResult List(int year, int count)
        {
            IDatabaseAccessor dbAccess = new SqlDatabaseAccessor(MvcApplication.GetMockDraftConnectionStringName());

            List<DProspect> dProspects = dbAccess.GetTopProspects(year, count);

            List<WProspect> prospects = new List<WProspect>();

            foreach (var dProspect in dProspects)
            {
                prospects.Add(Mapper.Map<WProspect>(dProspect));
            }

            return new JsonResult
            { 
                Data = prospects,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };
        }

        [HttpGet]
        public ActionResult Create()
        {
            var createProspectModel = new CreateProspectModel();
            return View(createProspectModel);
        }

        [HttpPost]
        public ActionResult Create(CreateProspectModel createProspectModel)
        {
            var prospectModel = createProspectModel.ProspectModel;
            prospectModel.Team = GetTeamWithId(createProspectModel.TeamId, createProspectModel.PossibleTeams);
            var dProspect = Mapper.Map<DProspect>(prospectModel);
            IDatabaseAccessor db = new SqlDatabaseAccessor(MvcApplication.GetMockDraftConnectionStringName());

            if (ModelState.IsValid)
            {
                if (db.ProspectExists(dProspect))
                {
                    ViewBag.Feedback = createProspectModel.AlreadyExistedErrorMessage;
                    return View(createProspectModel);
                }

                db.AddProspect(dProspect);
                ViewBag.Feedback = createProspectModel.SuccessMessage;
            }

            var newProspectModel = new CreateProspectModel();
            return View(newProspectModel);
        }

        public WTeam GetTeamWithId(int id, List<WTeam> teams)
        {
            foreach (var team in teams)
            {
                if (team.Id == id) return team;
            }

            return null;
        }
    }
}