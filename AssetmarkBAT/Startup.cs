using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AssetmarkBAT.Startup))]
namespace AssetmarkBAT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
