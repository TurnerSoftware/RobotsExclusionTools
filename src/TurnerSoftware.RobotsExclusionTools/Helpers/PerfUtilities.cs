using System;
using System.Buffers;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

internal static class PerfUtilities
{
	public static bool TryParseInteger(ReadOnlySpan<byte> source, out int value)
	{
		if (source.Length == 0)
		{
			value = 0;
			return false;
		}
		else if (source.Length < 3 && source[0] is >= (byte)'0' and <= (byte)'9')
		{
			value = source[0] - '0';
			if (source.Length == 2 && source[1] is >= (byte)'0' and <= (byte)'9')
			{
				value = (value * 10) + (source[1] - '0');
			}
			return true;
		}

#if NETSTANDARD2_0
		var valueString = Encoding.UTF8.GetString(source.ToArray());
		return int.TryParse(valueString, out value);
#else
		//We can have allocation-free integer parsing in .NET Standard 2.1
		var numberOfChars = Encoding.UTF8.GetCharCount(source);
		var charArray = ArrayPool<char>.Shared.Rent(numberOfChars);
		try
		{
			Encoding.UTF8.GetChars(source, charArray);
			return int.TryParse(charArray.AsSpan()[..numberOfChars], out value);
		}
		finally
		{
			ArrayPool<char>.Shared.Return(charArray);
		}
#endif
	}
}
