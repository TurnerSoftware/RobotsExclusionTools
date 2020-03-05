using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace TurnerSoftware.RobotsExclusionTools.Tests.TestSite
{
	public class Startup
	{
		private SiteContext Context { get; }

		public Startup(SiteContext context)
		{
			Context = context;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore();
		}

		public void Configure(IApplicationBuilder app)
		{
			if (Context.StatusCode == 200)
			{
				app.UseStaticFiles(new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(
						Path.Combine(Directory.GetCurrentDirectory(), "Resources"))
				});
			}
			else
			{
				app.UseMvc();
			}
		}
	}
}
