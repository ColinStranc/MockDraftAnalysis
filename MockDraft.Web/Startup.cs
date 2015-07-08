using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MockDraft.Web.Startup))]
namespace MockDraft.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
