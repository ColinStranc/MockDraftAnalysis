using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using DatabaseModels;
using MockDraft.Web.Models;

namespace MockDraft.Web
{
    public class MvcApplication : HttpApplication
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
            Mapper.CreateMap<DLeague, WLeague>();
            Mapper.CreateMap<DTeam, WTeam>();
            Mapper.CreateMap<DProspect, WProspect>();

            Mapper.CreateMap<DDraft, WDraft>();
            Mapper.CreateMap<DDraftPick, WDraftPick>();

            // W(eb) too D(atabase)
            Mapper.CreateMap<WLeague, DLeague>();
            Mapper.CreateMap<WTeam, DTeam>();
            Mapper.CreateMap<WProspect, DProspect>();

            Mapper.CreateMap<WDraft, DDraft>();
            Mapper.CreateMap<WDraftPick, DDraftPick>();
        }
    }
}
