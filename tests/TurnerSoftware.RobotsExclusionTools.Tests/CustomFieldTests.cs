using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tests;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class CustomFieldTests : TestBase
	{
		[TestMethod]
		public void CrawlDelayApplied()
		{
			var robotsFile = GetRobotsFile("Comprehensive-Example.txt");
			var userAgent = "AnyUserAgent";

			Assert.AreEqual(60, robotsFile.GetEntryForUserAgent(userAgent).CrawlDelay);
		}

		[TestMethod]
		public void SitemapsFromRobots()
		{
			var robotsFile = GetRobotsFile("Comprehensive-Example.txt");

			Assert.AreEqual(2, robotsFile.SitemapEntries.Count());
			Assert.AreEqual("http://www.example.org/sitemap.xml", robotsFile.SitemapEntries.First().Sitemap.ToString());
			Assert.AreEqual("http://www.example.org/sitemap2.xml", robotsFile.SitemapEntries.Last().Sitemap.ToString());
		}
	}
}
