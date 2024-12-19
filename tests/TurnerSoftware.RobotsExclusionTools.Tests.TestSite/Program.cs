using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using TurnerSoftware.RobotsExclusionTools.Tests.TestSite;


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRouting();


app.MapGet("robots.txt", async (ctx) =>
{
	var context = ctx.RequestServices.GetRequiredService<SiteContext>();
	if (context.ExpectedStatusCode == 200)
	{
		ctx.Response.Headers.ContentType = new("text/plain");
		ctx.Response.StatusCode = 200;
		await ctx.Response.WriteAsync("# This file is used by the Test Server\n# This allows testing loading by URI\nUser-agent: MyCustom-UserAgent\nDisallow: ");
	}
	else
	{
		ctx.Response.StatusCode = context.ExpectedStatusCode;
	}
	await ctx.Response.CompleteAsync();

});

app.Run();
public partial class Program { }