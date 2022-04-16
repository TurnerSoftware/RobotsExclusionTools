using System;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

/// <summary>
/// Various helper methods following the Robots.txt parsing rules 
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
	/// Whether a US-ASCII code page character is part of "escape".
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
	/// Attempts to parse an agent following the NoRobots RFC syntax rules.
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// agent		= 1*&lt;any CHAR except CTLs or tspecials&gt;
	/// CHAR		= &lt;any US-ASCII character (octets 0 - 127)&gt;
	/// CTL		= &lt;any US-ASCII control character (octets 0 - 31) and DEL (127)&gt;
	/// tspecials	= "(" | ")" | "&lt;" | "&gt;" | "@"
	///			| "," | ";" | ":" | "\" | &lt;"&gt;
	///			| "/" | "[" | "]" | "?" | "="
	///			| "{" | "}" | SP | HT
	///	</code>
	/// </remarks>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool TryParseAgent(ReadOnlySpan<char> input, out ReadOnlySpan<char> agent)
	{
		//The first and last non-CTL CHAR values
		const byte ValidCharMin = 32;
		const byte ValidCharMax = 126;

		var index = 0;
		for (; index < input.Length; index++)
		{
			var current = input[index];
			//Check we are between the CHAR range without being any CTL characters.
			if (current < ValidCharMin || current > ValidCharMax)
			{
				//Characters outside of our valid range are impossible to handle
				agent = ReadOnlySpan<char>.Empty;
				return false;
			}

			//Agents can't contain whitespace so we check if it trailing whitespace.
			//If it is only whitespace, we can safely trim it and return successfully.
			//Whitespace is also part of the "tspecials" check so this must occur before that.
			if (current == ' ' || current == '\t')
			{
				var localIndex = index + 1;
				for (; localIndex < input.Length; localIndex++)
				{
					current = input[localIndex];
					switch (current)
					{
						case ' ':
						case '\t':
							continue;
						default:
							agent = ReadOnlySpan<char>.Empty;
							return false;
					}
				}
				goto ValidAgent;
			}

			if (IsTSpecial(current))
			{
				//There is no safe path handling this rule any further
				agent = ReadOnlySpan<char>.Empty;
				return false;
			}
		}

		ValidAgent:
		agent = input.Slice(0, index);
		return true;
	}

	/// <summary>
	/// Attempts to parse a path ("path" and "rpath") following the NoRobots RFC syntax rules.
	/// This won't enforce a minimum number of characters between slashes.
	/// (eg. "//" is a valid path)
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// rpath		= "/" path
	/// path		= fsegment *( "/" segment )
	/// fsegment	= 1*pchar
	/// segment	=  *pchar
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
	/// <param name="input"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public static bool TryParsePath(ReadOnlySpan<char> input, out ReadOnlySpan<char> path)
	{
		var index = 0;
		for (; index < input.Length; index++)
		{
			var current = input[index];
			switch (current)
			{
				case '/':
					continue;
				case '%':
					//As part of "uchar" (escape)
					if (IsEscape(input.Slice(index)))
					{
						//Escape sequences are 3 characters, move the index 2 extra forwards
						index += 2;
						continue;
					}
					path = ReadOnlySpan<char>.Empty;
					return false;
				case ':':
				case '@':
				case '&':
				case '=':
					//As part of "pchar" (additional)
					continue;
				default:
					//As part of "pchar" (unreserved)
					if (IsUnreserved(current))
					{
						continue;
					}
					path = ReadOnlySpan<char>.Empty;
					return false;
			}
		}

		path = input.Slice(0, index);
		return true;
	}
}
