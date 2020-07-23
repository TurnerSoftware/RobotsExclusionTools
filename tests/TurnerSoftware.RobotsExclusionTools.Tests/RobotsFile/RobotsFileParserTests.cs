using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tests.TestSite;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RobotsFileParserTests : TestBase
	{
		private TestSiteManager GetRobotsSiteManager(int statusCode)
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
		public async Task FromUriLoading_404_NotFound_AllowAll()
		{
			using (var siteManager = GetRobotsSiteManager(404))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
				Assert.IsFalse(robotsFile.SiteAccessEntries.Any());
			}
		}

		[TestMethod]
		public async Task FromUriLoading_401_Unauthorized_DenyAll()
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

		[TestMethod]
		public async Task FromUriLoading_403_Forbidden_DenyAll()
		{
			using (var siteManager = GetRobotsSiteManager(403))
			{
				var client = siteManager.GetHttpClient();
				var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
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
		public async Task FromStreamLoading()
		{
			using (var fileStream = new FileStream("Resources/RobotsFile/NoRobots-RFC-Example.txt", FileMode.Open))
			{
				var robotsFile = await new RobotsFileParser().FromStreamAsync(fileStream, new Uri("http://www.example.org/"));
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any());
			}
		}

		[TestMethod]
		public async Task FromUriLoading_Cancellation()
		{
			using (var fileStream = new FileStream("Resources/RobotsFile/NoRobots-RFC-Example.txt", FileMode.Open))
			{
				var cts = new CancellationToken(true);
				await Assert.ThrowsExceptionAsync<OperationCanceledException>(
					async () => await new RobotsFileParser().FromStreamAsync(fileStream, new Uri("http://www.example.org/"), cts)
				);
			}
		}

		[TestMethod]
		public async Task FromStreamLoading_Cancellation()
		{
			using (var fileStream = new FileStream("Resources/RobotsFile/NoRobots-RFC-Example.txt", FileMode.Open))
			{
				var cts = new CancellationToken(true);
				await Assert.ThrowsExceptionAsync<OperationCanceledException>(
					async () => await new RobotsFileParser().FromStreamAsync(fileStream, new Uri("http://www.example.org/"), cts)
				);
			}
		}
	}
}
