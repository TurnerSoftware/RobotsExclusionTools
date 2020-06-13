using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RobotsFileTests : TestBase
	{
		[TestMethod]
		public void AllowAll()
		{
			var allowAllRobots = RobotsExclusionTools.RobotsFile.AllowAllRobots(new Uri("http://www.example.org/"));
			Assert.AreEqual(new Uri("http://www.example.org/"), allowAllRobots.BaseUri);
			Assert.AreEqual(0, allowAllRobots.SiteAccessEntries.Count());

			Assert.IsTrue(allowAllRobots.IsAllowedAccess(new Uri("http://www.example.org/index.html"), "Unhipbot/1.0"));
		}
		
		[TestMethod]
		public void DenyAll()
		{
			var denyAllRobots = RobotsExclusionTools.RobotsFile.DenyAllRobots(new Uri("http://www.example.org/"));
			Assert.AreEqual(new Uri("http://www.example.org/"), denyAllRobots.BaseUri);
			Assert.AreEqual(1, denyAllRobots.SiteAccessEntries.Count());

			var entry = denyAllRobots.SiteAccessEntries.First();
			Assert.IsTrue(entry.UserAgents.Contains("*"));

			var pathRule = entry.PathRules.First();
			Assert.AreEqual("/", pathRule.Path);
			Assert.AreEqual(PathRuleType.Disallow, pathRule.RuleType);

			Assert.IsFalse(denyAllRobots.IsAllowedAccess(new Uri("http://www.example.org/index.html"), "Unhipbot/1.0"));
		}

		[TestMethod]
		public void AnyAllowedInBlankFile()
		{
			var robotsFile = new RobotsExclusionTools.RobotsFile(new Uri("http://www.example.org/"));
			Assert.AreEqual(0, robotsFile.SiteAccessEntries.Count());
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), "GoogleBot"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/any"), "Webcrawler/1.0"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/path"), "Unhipbot/1.0"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/works"), "SuperSpecialCrawler/1.0"));
		}

		[TestMethod]
		public void IsAllowedAccessViaRelativeUris()
		{
			var robotsFile = GetRfcRobotsFile();
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/", UriKind.Relative), "Excite/1.0"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/server.html", UriKind.Relative), "Excite/1.0"));
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("/index.html", UriKind.Relative), "Unhipbot/1.0"));
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("/server.html", UriKind.Relative), "Unhipbot/1.0"));
		}

		[TestMethod]
		public void FieldCasing()
		{
			var robotsFile = GetRobotsFile("FieldCasing-Example.txt");
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("/RFCCompliant", UriKind.Relative), "A"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/RFCCompliant", UriKind.Relative), "B"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/RFCCompliant", UriKind.Relative), "C"));

			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseUserAgent", UriKind.Relative), "A"));
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseUserAgent", UriKind.Relative), "B"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseUserAgent", UriKind.Relative), "C"));

			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseRule", UriKind.Relative), "A"));
			Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseRule", UriKind.Relative), "B"));
			Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("/DifferentCaseRule", UriKind.Relative), "C"));
		}
	}
}
