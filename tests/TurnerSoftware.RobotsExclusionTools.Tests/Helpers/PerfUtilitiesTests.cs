using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Tests.Helpers;

[TestClass]
public class PerfUtilitiesTests
{
	[DataRow("", null)]
	[DataRow("0", 0)]
	[DataRow("9", 9)]
	[DataRow("10", 10)]
	[DataRow("99", 99)]
	[DataRow("100", 100)]
	[DataRow("999", 999)]
	[DataTestMethod]
	public void TryParseIntegerTest(string input, int? expected)
	{
		var bytes = Encoding.UTF8.GetBytes(input);
		var result = PerfUtilities.TryParseInteger(bytes, out var actual);
		if (expected is null)
		{
			Assert.IsFalse(result);
		}
		else
		{
			Assert.AreEqual(expected.Value, actual);
		}
	}
}
