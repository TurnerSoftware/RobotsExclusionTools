﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class CustomFieldTests : TestBase
	{
		[TestMethod]
		public void CrawlDelayApplied()
		{
			var robotsFile = GetRobotsFile("Comprehensive-Example.txt");
			var userAgent = "AnyUserAgent";

			Assert.IsTrue(robotsFile.TryGetEntryForUserAgent(userAgent, out var entry));
			Assert.AreEqual(60, entry.CrawlDelay);
		}

		[TestMethod]
		public void SitemapsFromRobots()
		{
			var robotsFile = GetRobotsFile("Comprehensive-Example.txt");

			Assert.AreEqual(2, robotsFile.SitemapEntries.Count);
			Assert.AreEqual("http://www.example.org/sitemap.xml", robotsFile.SitemapEntries.First().Sitemap.ToString());
			Assert.AreEqual("http://www.example.org/sitemap2.xml", robotsFile.SitemapEntries.Last().Sitemap.ToString());
		}
	}
}
