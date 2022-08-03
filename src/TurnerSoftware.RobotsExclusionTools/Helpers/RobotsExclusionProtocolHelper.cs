using System;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

public static class RobotsExclusionProtocolHelper
{
	/// <summary>
	/// Attempts to count the bytes of the first UTF-8 character from the byte span.
	/// </summary>
	/// <remarks>
	/// <b>UTF-8 (RFC 3629)</b>
	/// <code>
	/// UTF8-octets	= *( UTF8-char )
	/// UTF8-char	= UTF8-1 / UTF8-2 / UTF8-3 / UTF8-4
	/// UTF8-1		= %x00-7F
	/// UTF8-2		= %xC2-DF UTF8-tail
	/// UTF8-3		= %xE0 %xA0-BF UTF8-tail / %xE1-EC 2( UTF8-tail ) /
	/// 			%xED %x80-9F UTF8-tail / %xEE-EF 2( UTF8-tail )
	/// UTF8-4		= %xF0 %x90-BF 2( UTF8-tail ) / %xF1-F3 3( UTF8-tail ) /
	///				%xF4 %x80-8F 2( UTF8-tail )
	/// UTF8-tail	= %x80-BF
	/// </code>
	/// </remarks>
	/// <returns></returns>
	public static unsafe bool TryReadUtf8ByteSequence(ReadOnlySpan<byte> value, out int numberOfBytes)
	{
		byte byte1, byte2 = 0, byte3 = 0, byte4 = 0;
		if (value.Length > 0)
		{
			byte1 = value[0];
			if (value.Length > 1)
			{
				byte2 = value[1];
				if (value.Length > 2)
				{
					byte3 = value[2];
					if (value.Length > 3)
					{
						byte4 = value[3];
					}
				}
			}
		}
		else
		{
			numberOfBytes = 0;
			return false;
		}

		numberOfBytes = byte1 switch
		{
			<= 0x7F => 1,

			>= 0xC2 and <= 0xDF when byte2 is >= 0x80 and <= 0xBF => 2,

			0xE0 when byte2 is >= 0xA0 and <= 0xBF && byte3 is >= 0x80 and <= 0xBF => 3,
			>= 0xE1 and <= 0xEC when byte2 is >= 0x80 and <= 0xBF && byte3 is >= 0x80 and <= 0xBF => 3,
			0xED when byte2 is >= 0x80 and <= 0x9F && byte3 is >= 0x80 and <= 0xBF => 3,
			>= 0xEE and <= 0xEF when byte2 is >= 0x80 and <= 0xBF && byte3 is >= 0x80 and <= 0xBF => 3,


			0xF0 when byte2 is >= 0x90 and <= 0xBF && byte3 is >= 0x80 and <= 0xBF && byte4 is >= 0x80 and <= 0xBF => 4,
			>= 0xF1 and <= 0xF3 when byte2 is >= 0x80 and <= 0xBF && byte3 is >= 0x80 and <= 0xBF && byte4 is >= 0x80 and <= 0xBF => 4,
			0xF4 when byte2 is >= 0x80 and <= 0x8F && byte3 is >= 0x80 and <= 0xBF && byte4 is >= 0x80 and <= 0xBF => 4,

			_ => -1
		};
		return numberOfBytes != -1;
	}

	/// <summary>
	/// Checks whether the value satisfies the Robots Exclusion Protocol "product-token" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// product-token	= identifier / "*"
	/// identifier	= 1*(%x2D / %x41-5A / %x5F / %x61-7A)
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool IsValidProductToken(ReadOnlySpan<byte> value)
	{
		if (value.Length == 1 && value[0] == '*')
		{
			return true;
		}
		return IsValidIdentifier(value);
	}

	/// <summary>
	/// Checks whether the value satisfies the Robots Exclusion Protocol "identifier" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// identifier	= 1*(%x2D / %x41-5A / %x5F / %x61-7A)
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool IsValidIdentifier(ReadOnlySpan<byte> value)
	{
		for (var i = 0; i < value.Length; i++)
		{
			var isValidIdentifier = (ushort)value[i] switch
			{
				0x2D => true,
				>= 0x41 and <= 0x5A => true,
				0x5F => true,
				>= 0x61 and <= 0x7A => true,
				_ => false
			};

			if (!isValidIdentifier)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Checks whether the value satisfies the Robots Exclusion Protocol "path-pattern" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// path-pattern		= "/" *UTF8-char-noctl ; valid URI path pattern
	/// UTF8-char-noctl	= UTF8-1-noctl / UTF8-2 / UTF8-3 / UTF8-4
	/// UTF8-1-noctl		= %x21 / %x22 / %x24-7F ; excluding control, space, '#'
	/// UTF8-2			= %xC2-DF UTF8-tail
	/// UTF8-3			= %xE0 %xA0-BF UTF8-tail / %xE1-EC 2UTF8-tail /
	///					%xED %x80-9F UTF8-tail / %xEE-EF 2UTF8-tail
	/// UTF8-4			= %xF0 %x90-BF 2UTF8-tail / %xF1-F3 3UTF8-tail /
	///					%xF4 %x80-8F 2UTF8-tail
	/// </code>
	/// </remarks>
	/// <returns></returns>
	public static bool IsValidPath(ReadOnlySpan<byte> value)
	{
		if (value.Length > 0 && value[0] == '/')
		{
			return true;
		}

		for (var i = 1; i < value.Length; i++)
		{
			var offsetValue = value.Slice(i);
			if (!TryReadUtf8ByteSequence(offsetValue, out _))
			{
				return false;
			}
		}

		return true;
	}
}
