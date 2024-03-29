﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tests.TestSite;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RobotsFileParserTests : TestBase
	{
		private static TestSiteManager GetRobotsSiteManager(int statusCode)
		{
			return new TestSiteManager(new SiteContext { StatusCode = statusCode });
		}

		[TestMethod]
		public async Task FromUriLoading_200_OK_PerFileRules()
		{
			using (var siteManager = GetRobotsSiteManager(200))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("MyCustom-UserAgent") && 
					s.PathRules.Any(p => string.IsNullOrEmpty(p.Path) && p.RuleType == PathRuleType.Disallow)
				));
			}
		}

		[TestMethod]
		public async Task FromUriLoading_AccessRules_404_NotFound_AllowAll()
		{
			using (var siteManager = GetRobotsSiteManager(404))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen404NotFound = true
				});
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}
		[TestMethod]
		public async Task FromUriLoading_AccessRules_404_NotFound_DenyAll()
		{
			using (var siteManager = GetRobotsSiteManager(404))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen404NotFound = false
				});
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("*") && s.PathRules.Any(p => p.Path == "/" && p.RuleType == PathRuleType.Disallow)
				));
			}
		}

		[TestMethod]
		public async Task FromUriLoading_AccessRules_401_Unauthorized_AllowAll()
		{
			using (var siteManager = GetRobotsSiteManager(401))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen401Unauthorized = true
				});
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}
		[TestMethod]
		public async Task FromUriLoading_AccessRules_401_Unauthorized_DenyAll()
		{
			using (var siteManager = GetRobotsSiteManager(401))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen401Unauthorized = false
				});
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("*") && s.PathRules.Any(p => p.Path == "/" && p.RuleType == PathRuleType.Disallow)
				));
			}
		}

		[TestMethod]
		public async Task FromUriLoading_AccessRules_403_Forbidden_AllowAll()
		{
			using (var siteManager = GetRobotsSiteManager(403))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen403Forbidden = true
				});
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}
		[TestMethod]
		public async Task FromUriLoading_AccessRules_403_Forbidden_DenyAll()
		{
			using (var siteManager = GetRobotsSiteManager(403))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"), new RobotsFileAccessRules
				{
					AllowAllWhen403Forbidden = false
				});
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("*") && s.PathRules.Any(p => p.Path == "/" && p.RuleType == PathRuleType.Disallow)
				));
			}
		}

		[TestMethod]
		public async Task FromUriLoading_OtherHttpStatus_AllowAll()
		{
			using (var siteManager = GetRobotsSiteManager(418))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}

		[TestMethod]
		public async Task FromUriLoading_DefaultNoRobotsRFCRules()
		{
			using (var siteManager = GetRobotsSiteManager(401))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("*") && s.PathRules.Any(p => p.Path == "/" && p.RuleType == PathRuleType.Disallow)
				));
			}
		}
	}
}
