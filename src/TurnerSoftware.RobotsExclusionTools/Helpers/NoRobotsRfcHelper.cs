using System;
using System.Runtime.CompilerServices;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

/// <summary>
/// Various helper methods following the Robots.txt syntax rules 
/// set out in the NoRobots RFC.
/// </summary>
public static class NoRobotsRfcHelper
{
	/// <summary>
	/// Whether a US-ASCII code page character is part of "tspecials".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// tspecials	= "(" | ")" | "&lt;" | "&gt;" | "@"
	///			| "," | ";" | ":" | "\" | &lt;"&gt;
	///			| "/" | "[" | "]" | "?" | "="
	///			| "{" | "}" | SP | HT
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTSpecial(char value) => value switch
	{
		'(' or ')' or '<' or '>' or '@' or
			',' or ';' or ':' or '\\' or '"' or
			'/' or '[' or ']' or '?' or '=' or
			'{' or '}' or ' ' or '\t' => true,
		_ => false
	};

	/// <summary>
	/// Whether a US-ASCII code page character is part of "unreserved".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// unreserved	= alpha | digit | safe | extra
	/// alpha		= lowalpha | hialpha
	/// lowalpha	= "a" | "b" | "c" | "d" | "e" | "f" | "g" | "h" | "i" |
	///			"j" | "k" | "l" | "m" | "n" | "o" | "p" | "q" | "r" |
	///			"s" | "t" | "u" | "v" | "w" | "x" | "y" | "z"
	/// hialpha	= "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" |
	///			"J" | "K" | "L" | "M" | "N" | "O" | "P" | "Q" | "R" |
	///			"S" | "T" | "U" | "V" | "W" | "X" | "Y" | "Z"
	/// digit		= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" |
	///			"8" | "9"
	/// safe		= "$" | "-" | "_" | "." | "+"
	/// extra		= "!" | "*" | "'" | "(" | ")" | ","
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUnreserved(char value) => value switch
	{
		//lowalpha
		>= 'a' and <= 'z' => true,
		//hialpha
		>= 'A' and <= 'Z' => true,
		//digit
		>= '0' and <= '9' => true,
		//safe
		'$' or '-' or '_' or '.' or '+' => true,
		//extra
		'!' or '*' or '\'' or '(' or ')' or ',' => true,
		_ => false
	};

	/// <summary>
	/// Whether a US-ASCII code page sequence is part of "escape".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// escape	= "%" hex hex
	/// hex	= digit | "A" | "B" | "C" | "D" | "E" | "F" |
	///		"a" | "b" | "c" | "d" | "e" | "f"
	/// digit	= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" |
	///		"8" | "9"
	///	</code>
	/// </remarks>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEscape(ReadOnlySpan<char> value)
	{
		if (value.Length > 3 && value[0] == '%')
		{
			for (var i = 1; i < 3; i++)
			{
				var isValid = value[i] switch
				{
					//lowalpha
					>= 'a' and <= 'f' => true,
					//hialpha
					>= 'A' and <= 'F' => true,
					//digit
					>= '0' and <= '9' => true,
					_ => false
				};

				if (!isValid)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Whether a US-ASCII code page sequence is part of "uchar".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// uchar		= unreserved | escape
	/// escape		= "%" hex hex
	/// hex		= digit | "A" | "B" | "C" | "D" | "E" | "F" |
	///                      "a" | "b" | "c" | "d" | "e" | "f"
	/// unreserved	= alpha | digit | safe | extra
	/// alpha		= lowalpha | hialpha
	/// lowalpha	= "a" | "b" | "c" | "d" | "e" | "f" | "g" | "h" | "i" |
	///			"j" | "k" | "l" | "m" | "n" | "o" | "p" | "q" | "r" |
	///			"s" | "t" | "u" | "v" | "w" | "x" | "y" | "z"
	/// hialpha	= "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" |
	///			"J" | "K" | "L" | "M" | "N" | "O" | "P" | "Q" | "R" |
	///			"S" | "T" | "U" | "V" | "W" | "X" | "Y" | "Z"
	/// digit		= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" |
	///			"8" | "9"
	/// safe		= "$" | "-" | "_" | "." | "+"
	/// extra		= "!" | "*" | "'" | "(" | ")" | ","
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <param name="isEscapeSequence"></param>
	/// <returns></returns>
	public static bool IsUChar(ReadOnlySpan<char> value, out bool isEscapeSequence)
	{
		isEscapeSequence = false;
		if (value.Length > 0)
		{
			var current = value[0];

			//As part of "uchar" (escape)
			if (current == '%')
			{
				isEscapeSequence = IsEscape(value);
				return isEscapeSequence;
			}

			//As part of "uchar" (unreserved)
			return IsUnreserved(current);
		}
		return false;
	}

	/// <summary>
	/// Whether a US-ASCII code page sequence is part of "pchar".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// pchar		= uchar | ":" | "@" | "&amp;" | "="
	/// uchar		= unreserved | escape
	/// escape		= "%" hex hex
	/// hex		= digit | "A" | "B" | "C" | "D" | "E" | "F" |
	///                      "a" | "b" | "c" | "d" | "e" | "f"
	/// unreserved	= alpha | digit | safe | extra
	/// alpha		= lowalpha | hialpha
	/// lowalpha	= "a" | "b" | "c" | "d" | "e" | "f" | "g" | "h" | "i" |
	///			"j" | "k" | "l" | "m" | "n" | "o" | "p" | "q" | "r" |
	///			"s" | "t" | "u" | "v" | "w" | "x" | "y" | "z"
	/// hialpha	= "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" |
	///			"J" | "K" | "L" | "M" | "N" | "O" | "P" | "Q" | "R" |
	///			"S" | "T" | "U" | "V" | "W" | "X" | "Y" | "Z"
	/// digit		= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" |
	///			"8" | "9"
	/// safe		= "$" | "-" | "_" | "." | "+"
	/// extra		= "!" | "*" | "'" | "(" | ")" | ","
	/// </code>
	/// </remarks>
	/// <param name="value"></param>
	/// <param name="isEscapeSequence"></param>
	/// <returns></returns>
	public static bool IsPChar(ReadOnlySpan<char> value, out bool isEscapeSequence)
	{
		isEscapeSequence = false;
		if (value.Length > 0)
		{
			var current = value[0];
			return current switch
			{
				//As part of "pchar" (additional)
				':' or '@' or '&' or '=' => true,
				//As part of "pchar" (uchar)
				_ => IsUChar(value, out isEscapeSequence),
			};
		}
		return false;
	}
}
