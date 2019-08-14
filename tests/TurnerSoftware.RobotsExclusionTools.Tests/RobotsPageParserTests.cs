using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests
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
