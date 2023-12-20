using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AdmWebTest2.Startup))]
namespace AdmWebTest2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
