[assembly: HostingStartup(typeof(blog.Areas.Identity.IdentityHostingStartup))]
namespace blog.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}