using System;
using System.Collections.Generic;
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
	}
}
