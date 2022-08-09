using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class PathWildcardTests : TestBase
	{
		[TestMethod]
		public async Task ExplicitWildcardSuffix()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "ExplicitWildcardSuffix";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/organization/plan.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task ExplicitWildcardPrefix()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "ExplicitWildcardPrefix";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/organization/plan.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/organization/plan.jpg"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/plan.html"), userAgent));
			});
		}
		
		[TestMethod]
		public async Task ContainedWildcard()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "ContainedWildcard";

				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/organization/plan.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task PathWithAnyQueryString()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "PathWithAnyQueryString";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo=bar"), userAgent));
			});
		}

		[TestMethod]
		public async Task PathWithPartQueryString()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "PathWithPartQueryString";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo="), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo=bar"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo=bar&abc=123"), userAgent));
			});
		}

		[TestMethod]
		public async Task PathMustStartWith()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "PathMustStartWith";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/about/org/plan.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
			});
		}

		[TestMethod]
		public async Task PathMustEndWith()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "PathMustEndWith";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo="), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/planb.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/planb.html?foo=bar"), userAgent));
			});
		}

		[TestMethod]
		public async Task OnlyWildcard()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "OnlyWildcard";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/about.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html?foo="), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/planb.html"), userAgent));
				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/planb.html?foo=bar"), userAgent));
			});
		}

		[TestMethod]
		public async Task DoubleWildcard()
		{
			await WithRobotsFileAsync("Comprehensive-Example.txt", robotsFile =>
			{
				var userAgent = "DoubleWildcard";

				Assert.IsTrue(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/plan.html"), userAgent));
				Assert.IsFalse(robotsFile.IsAllowedAccess(new Uri("http://www.example.org/org/secret/plan.html"), userAgent));
			});
		}
	}
}
