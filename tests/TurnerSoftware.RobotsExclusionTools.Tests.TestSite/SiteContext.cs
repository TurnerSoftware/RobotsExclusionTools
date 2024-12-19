namespace TurnerSoftware.RobotsExclusionTools.Tests.TestSite
{
	public class SiteContext(int expectedStatusCode)
	{
		public int ExpectedStatusCode { get; set; } = expectedStatusCode;
	}
}
