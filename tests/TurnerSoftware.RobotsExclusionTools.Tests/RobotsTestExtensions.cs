using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	public static class RobotsTestExtensions
	{
		public static void AssertAccess(this RobotsExclusionTools.RobotsFile robotsFile, bool isAllowed, string userAgent, string url)
		{
			Assert.AreEqual(
				isAllowed,
				robotsFile.IsAllowedAccess(
					new Uri(url),
					userAgent
				)
			);
		}
	}
}
