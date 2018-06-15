using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(lampbae_final_project.Startup))]
namespace lampbae_final_project
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
