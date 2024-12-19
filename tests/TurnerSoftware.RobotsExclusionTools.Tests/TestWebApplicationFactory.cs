using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using TurnerSoftware.RobotsExclusionTools.Tests.TestSite;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	internal class TestWebApplicationFactory<TProgram>(int expectedStatusCode) : WebApplicationFactory<TProgram>
		where TProgram : class
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				services.Add(
					new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(SiteContext), (sp) => new SiteContext(expectedStatusCode), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)
					);
			});
		}
	}
}
