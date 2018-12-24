using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class RobotsFileTests : TestBase
	{
		[TestMethod]
		public void RFCRobotsFileAlwaysAllowed()
		{
			var robotsFile = GetRFCRobotsFile();
			var comparisonUtility = new PathComparisonUtility();

			var requestUri = new Uri("http://www.example.org/robots.txt");

			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Unhipbot/2.5"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "Excite/1.0"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "webcrawler"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(requestUri, "SuperSpecialCrawler"));
		}

		[TestMethod]
		public void RFCUnhipbotCrawlerDisallowed()
		{
			var robotsFile = GetRFCRobotsFile();
			var comparisonUtility = new PathComparisonUtility();

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
		public void RFCExciteCrawlerAllowed()
		{
			var robotsFile = GetRFCRobotsFile();
			var comparisonUtility = new PathComparisonUtility();

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
		public void RFCWebcrawlerAllowed()
		{
			var robotsFile = GetRFCRobotsFile();
			var comparisonUtility = new PathComparisonUtility();

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
		public void RFCOtherPartiallyAllowed()
		{
			var robotsFile = GetRFCRobotsFile();
			var comparisonUtility = new PathComparisonUtility();

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
