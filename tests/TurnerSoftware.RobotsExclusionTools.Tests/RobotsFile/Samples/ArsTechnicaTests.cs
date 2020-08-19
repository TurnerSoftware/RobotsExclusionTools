using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsFile.Samples
{
	[TestClass]
	public class ArsTechnicaTests : TestBase
	{
		[DataTestMethod]
		[DataRow("PostmanRuntime/7.26.2", true)]
		[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36", true)]
		[DataRow("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)", true)]
		public void Article(string userAgent, bool isAllowed)
		{
			var robots = GetSampleRobotsFile("arstechnica.com");

			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/information-technology/2020/08/the-golden-age-of-computer-user-groups/");
			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/gadgets/2020/08/surface-duo-internals-show-microsofts-fanatical-commitment-to-thinness/");
			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/science/2020/08/could-a-dragon-spacecraft-fly-humans-to-the-moon-its-complicated/");
		}

		[DataTestMethod]
		[DataRow("PostmanRuntime/7.26.2", false)]
		[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36", false)]
		[DataRow("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)", false)]
		public void GlobalDisallowed(string userAgent, bool isAllowed)
		{
			var robots = GetSampleRobotsFile("arstechnica.com");

			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/wp-admin/");
			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/civis/images/");
			robots.AssertAccess(isAllowed, userAgent,
				"https://arstechnica.com/category/foo/bar");
		}
	}
}
