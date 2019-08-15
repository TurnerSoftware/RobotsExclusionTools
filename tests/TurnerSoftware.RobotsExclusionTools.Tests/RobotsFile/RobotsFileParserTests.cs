using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile
{
	[TestClass]
	public class RobotsFileParserTests : TestBase
	{
		[TestMethod]
		public async Task FromUriLoading()
		{
			var client = TestConfiguration.GetHttpClient();
			var robotsFile = await new RobotsFileParser(client).FromUriAsync(new Uri("http://localhost/robots.txt"));
			Assert.IsTrue(robotsFile.SiteAccessEntries.Any());
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
	}
}
