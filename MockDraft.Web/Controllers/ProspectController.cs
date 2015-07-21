﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockDraft.Web.Models;
using AutoMapper;
using Database;

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
            IDatabaseAccessor dbAccess = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

            List<DatabaseModels.DProspect> dProspects = dbAccess.GetTopProspects(year, count);

            List<WProspect> prospects = new List<WProspect>();

            foreach (var dProspect in dProspects)
            {
                prospects.Add(Mapper.Map<WProspect>(dProspect));
            }

            return new JsonResult() { 
                Data = prospects,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };
        }
    }
}