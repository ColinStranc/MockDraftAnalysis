using System.Web.Mvc;
using MockDraft.Web.Models;

namespace MockDraft.Web.Controllers
{
    public class MockDraftController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Details(CreateDraftModel draft)
        {
            return RedirectToAction("Create", "MockDraft", new { draftModel = draft });
        }

        [HttpGet]
        public ActionResult Create(CreateDraftModel draft)
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult Create(CreateDraftModel draft)
        //{
        //    draft.Draft.DraftLength = draft.Picks.Count;
        //}
    }
}