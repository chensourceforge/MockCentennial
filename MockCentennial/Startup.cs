using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MockCentennial.Startup))]
namespace MockCentennial
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
