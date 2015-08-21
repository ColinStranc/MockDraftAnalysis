using System.Web.Mvc;
using AutoMapper;
using Database;
using DatabaseModels;
using MockDraft.Web.Models;

namespace MockDraft.Web.Controllers
{
    public class LeagueController : Controller
    {
        [HttpGet]
        public ActionResult Create()
        {
            var league = new CreateLeagueModel();
            return View(league);
        }

        [HttpPost]
        public ActionResult Create(CreateLeagueModel createLeagueModel)
        {
            var league = createLeagueModel.LeagueModel;

            var dLeague = Mapper.Map<DLeague>(league);
            IDatabaseAccessor db = new SqlDatabaseAccessor(MvcApplication.GetMockDraftConnectionStringName());

            if (ModelState.IsValid)
            {
                if (db.LeagueNameExists(league.Name))
                {
                    ViewBag.Feedback = createLeagueModel.AlreadyExistedErrorMessage;
                    return View(createLeagueModel);
                }

                db.AddLeague(dLeague);
                ViewBag.Feedback = createLeagueModel.SuccessMessage;
            }

            return View();
        }
    }
}