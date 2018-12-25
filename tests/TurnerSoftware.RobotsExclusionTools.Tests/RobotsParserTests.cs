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
	public class RobotsParserTests : TestBase
	{
		[TestMethod]
		public async Task FromUriLoading()
		{
			var robotsFile = await new RobotsParser().FromUriAsync(new Uri("https://github.com/robots.txt"));
			Assert.IsTrue(robotsFile.SiteAccessEntries.Any());
		}

		[TestMethod]
		public async Task FromStreamLoading()
		{
			using (var fileStream = new FileStream("Resources/NoRobots-RFC-Example.txt", FileMode.Open))
			{
				var robotsFile = await new RobotsParser().FromStreamAsync(fileStream, new Uri("http://www.example.org/"));
				Assert.IsTrue(robotsFile.SiteAccessEntries.Any());
			}
		}
	}
}
