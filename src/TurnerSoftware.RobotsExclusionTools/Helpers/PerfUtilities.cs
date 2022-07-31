using System;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

internal static class PerfUtilities
{
	public static bool TryParseInteger(ReadOnlySpan<char> source, out int value)
	{
		if (source.Length == 0)
		{
			value = 0;
			return false;
		}
		else if (source.Length < 3 && source[0] is >= '0' and <= '9')
		{
			value = source[0] - '0';
			if (source.Length == 2 && source[1] is >= '0' and <= '9')
			{
				value = (value * 10) + (source[1] - '0');
			}
			return true;
		}

#if NETSTANDARD2_0
		return int.TryParse(source.ToString(), out value);
#else
		//We can have allocation-free integer parsing in .NET Standard 2.1
		return int.TryParse(source, out value);
#endif
	}

}
