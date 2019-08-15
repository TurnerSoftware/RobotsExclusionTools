using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RFCSpecificTests : TestBase
	{
		[TestMethod]
		public void RobotsFileAlwaysAllowed()
		{
			var robotsFile = GetRfcRobotsFile();
			var requestUri = new Uri("http://www.example.org/robots.txt");

			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Unhipbot/2.5"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Excite/1.0"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "webcrawler"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "SuperSpecialCrawler"));
		}

		[TestMethod]
		public void DefaultAllUserAgentsWhenNoneExplicitlySet()
		{
			var robotsFile = GetRobotsFile("NoUserAgentRules-Example.txt");
			var userAgent = "RandomUserAgent";

			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
		}

		[TestMethod]
		public void UnhipbotCrawlerDisallowed()
		{
			var robotsFile = GetRfcRobotsFile();
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
		}

		[TestMethod]
		public void ExciteCrawlerAllowed()
		{
			var robotsFile = GetRfcRobotsFile();
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
		}

		[TestMethod]
		public void WebcrawlerAllowed()
		{
			var robotsFile = GetRfcRobotsFile();
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
		}

		[TestMethod]
		public void OtherPartiallyAllowed()
		{
			var robotsFile = GetRfcRobotsFile();
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
		}
	}
}
