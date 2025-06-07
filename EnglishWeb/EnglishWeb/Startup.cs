using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EnglishWeb.Startup))]
namespace EnglishWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
