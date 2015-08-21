using Microsoft.Owin;
using MockDraft.Web;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
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
