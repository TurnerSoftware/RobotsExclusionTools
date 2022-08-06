using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Tests.Helpers;

[TestClass]
public class RobotsExclusionProtocolHelperTests
{
	[DataRow(0, new byte[] { }, null)]
	[DataRow(1, new byte[] { 0x80 }, null)]
	[DataRow(2, new byte[] { 0x4D }, 1)]
	[DataRow(3, new byte[] { 0xC4, 0x9D }, 2)]
	[DataRow(4, new byte[] { 0xE0, 0xA0, 0x80 }, 3)]
	[DataRow(5, new byte[] { 0xE7, 0x80, 0x80 }, 3)]
	[DataRow(6, new byte[] { 0xED, 0x9F, 0xBF }, 3)]
	[DataRow(7, new byte[] { 0xEE, 0xBF, 0xBF }, 3)]
	[DataRow(8, new byte[] { 0xF0, 0x90, 0xBF, 0x80 }, 4)]
	[DataRow(9, new byte[] { 0xF2, 0xBF, 0xBF, 0x80 }, 4)]
	[DataRow(10, new byte[] { 0xF4, 0x8F, 0x80, 0x80 }, 4)]
	[DataTestMethod]
	public void TryReadUtf8ByteSequenceTest(int _, object[] rawInput, int? expectedNumberOfBytes)
	{
		var bytes = Array.ConvertAll(rawInput, (input) => (byte)Convert.ToInt32(input));
		var result = RobotsExclusionProtocolHelper.TryReadUtf8ByteSequence(bytes, out var actual);
		if (expectedNumberOfBytes is null)
		{
			Assert.IsFalse(result);
		}
		else
		{
			Assert.AreEqual(expectedNumberOfBytes.Value, actual);
		}
	}
}
