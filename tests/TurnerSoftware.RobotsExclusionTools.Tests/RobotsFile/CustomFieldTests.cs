using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class CustomFieldTests : TestBase
	{
		[TestMethod]
		public async Task CrawlDelayApplied()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "AnyUserAgent";

				Assert.IsTrue(robotsFile.TryGetEntryForUserAgent(userAgent, out var entry));
				Assert.AreEqual(60, entry.CrawlDelay);
			});
		}

		[TestMethod]
		public async Task SitemapsFromRobots()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				Assert.AreEqual(2, robotsFile.SitemapEntries.Count);
				Assert.AreEqual("http://www.example.org/sitemap.xml", robotsFile.SitemapEntries.First().Sitemap.ToString());
				Assert.AreEqual("http://www.example.org/sitemap2.xml", robotsFile.SitemapEntries.Last().Sitemap.ToString());
			});
		}
	}
}
