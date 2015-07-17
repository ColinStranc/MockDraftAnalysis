using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using DatabaseModels;

namespace MockDraft.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AutoMapperInit();
        }

        public static string GetMockDraftConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["MockDrafts"].ConnectionString;
        }

        public static string GetMockDraftConnectionStringName()
        {
            return "MockDrafts";
        }

        private void AutoMapperInit()
        {
            // D(atabase) too W(eb)
            Mapper.CreateMap<DatabaseModels.DLeague, MockDraft.Web.Models.WLeague>();
            Mapper.CreateMap<DatabaseModels.DTeam, MockDraft.Web.Models.WTeam>();
            Mapper.CreateMap<DatabaseModels.DProspect, MockDraft.Web.Models.WProspect>();
            
            
        }
    }
}
