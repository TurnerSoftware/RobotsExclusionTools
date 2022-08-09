using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RFCSpecificTests : TestBase
	{
		[TestMethod]
		public async Task RobotsFileAlwaysAllowed()
		{
			await WithRobotsFileAsync("NoRobots-RFC-Example.txt", robotsFile =>
			{
				var requestUri = new Uri("http://www.example.org/robots.txt");

				Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Unhipbot/2.5"));
				Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Excite/1.0"));
				Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "webcrawler"));
				Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "SuperSpecialCrawler"));
			});
		}

		[TestMethod]
		public async Task DefaultAllUserAgentsWhenNoneExplicitlySet()
		{
			await WithRobotsFileAsync("NoUserAgentRules-Example.txt", robotsFile =>
			{
				var userAgent = "RandomUserAgent";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task UnhipbotCrawlerDisallowed()
		{
			await WithRobotsFileAsync("NoRobots-RFC-Example.txt", robotsFile =>
			{
				var userAgent = "Unhipbot/1.0";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/index.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/server.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/fast.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/slow.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/orgo.gif"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plans.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Ejim/jim.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Emak/mak.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task ExciteCrawlerAllowed()
		{
			await WithRobotsFileAsync("NoRobots-RFC-Example.txt", robotsFile =>
			{
				var userAgent = "Excite/1.0";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/index.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/server.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/fast.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/slow.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/orgo.gif"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plans.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Ejim/jim.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Emak/mak.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task WebcrawlerAllowed()
		{
			await WithRobotsFileAsync("NoRobots-RFC-Example.txt", robotsFile =>
			{
				var userAgent = "Webcrawler/1.0";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/index.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/server.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/fast.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/slow.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/orgo.gif"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plans.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Ejim/jim.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Emak/mak.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task OtherPartiallyAllowed()
		{
			await WithRobotsFileAsync("NoRobots-RFC-Example.txt", robotsFile =>
			{
				var userAgent = "SuperSpecialCrawler/1.0";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/index.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/server.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/fast.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/services/slow.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/orgo.gif"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plans.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Ejim/jim.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/%7Emak/mak.html"), userAgent));
			});
		}
	}
}
