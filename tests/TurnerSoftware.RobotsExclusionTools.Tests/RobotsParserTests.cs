using System;
using System.Collections.Generic;
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
	}
}
