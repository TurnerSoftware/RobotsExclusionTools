using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsPage
{
	[TestClass]
	public class RobotsPageParserTests : TestBase
	{
		[TestMethod]
		public void FromRules()
		{
			var resource = LoadResource("RobotsPage/RobotsPage-Example.txt");
			var individualRules = resource.Split(new[] { '\n' });
			var robotsPageDefinition = new RobotsPageParser().FromRules(individualRules);

			Assert.IsTrue(robotsPageDefinition.PageAccessEntries.Any());
		}
	}
}
