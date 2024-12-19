using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RobotsFileParserTests : TestBase
	{
		private static WebApplicationFactory<Program> GetTestSiteFactory(int statusCode)
		{
			var factory = new TestWebApplicationFactory<Program>(statusCode);

			return factory;
		}

		[TestMethod]
		public async Task FromUriLoading_200_OK_PerFileRules()
		{
			using (var factory = GetTestSiteFactory(200))
			{
				var client = factory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(404))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(404))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(401))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(401))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(403))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(403))
			{
				var client = webFactory.CreateClient();
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
			using (var webFactory = GetTestSiteFactory(418))
			{
				var client = webFactory.CreateClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}

		[TestMethod]
		public async Task FromUriLoading_DefaultNoRobotsRFCRules()
		{
			using (var webFactory = GetTestSiteFactory(401))
			{
				var client = webFactory.CreateClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any(s =>
					s.UserAgents.Contains("*") && s.PathRules.Any(p => p.Path == "/" && p.RuleType == PathRuleType.Disallow)
				));
			}
		}
	}
}
