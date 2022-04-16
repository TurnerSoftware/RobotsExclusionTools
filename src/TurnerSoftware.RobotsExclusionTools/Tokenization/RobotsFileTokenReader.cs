using System;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public struct RobotsFileTokenReader
{
	private const char EndOfFile = char.MinValue;

	private readonly ReadOnlyMemory<char> Value;
	private int Index;

	public RobotsFileTokenReader(ReadOnlyMemory<char> value)
	{
		Value = value;
		Index = 0;
	}

	private char Current
	{
		get
		{
			if (Index < Value.Length)
			{
				return Value.Span[Index];
			}

			return EndOfFile;
		}
	}

	private char Peek()
	{
		if (Index + 1 < Value.Length)
		{
			return Value.Span[Index + 1];
		}
		return EndOfFile;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ReadNext() => Index++;

	public bool NextToken(out RobotsFileToken token, RobotsFileTokenValueFormat valueFormat = RobotsFileTokenValueFormat.Value)
	{
		if (Current == EndOfFile)
		{
			token = default;
			return false;
		}

		token = Current switch
		{
			' ' or '\t' => ReadWhitespace(),
			'#' => ReadComment(),
			'\r' or '\n' => ReadNewLine(),
			':' => ReadDelimiter(),
			_ => valueFormat switch
			{
				RobotsFileTokenValueFormat.Value => ReadValue(),
				RobotsFileTokenValueFormat.Token => ReadValueInTokenFormat(),
				RobotsFileTokenValueFormat.Path => ReadValueInPathFormat(enforceRPath: false),
				RobotsFileTokenValueFormat.RPath => ReadValueInPathFormat(enforceRPath: true),
				_ => ReadValue(),
			}
		};
		return true;
	}

	public void SkipLine()
	{
		if (Index < Value.Length)
		{
			var newLineIndex = Value.Span
				.Slice(Index)
				.IndexOfAny('\r', '\n');

			if (newLineIndex == -1)
			{
				//If no new line is found, we skip to the end
				Index = Value.Length;
			}
			else
			{
				Index = newLineIndex;
				if (Current == '\r' && Peek() == '\n')
				{
					ReadNext();
				}
				ReadNext();
			}
		}
	}

	private RobotsFileToken CreateToken(RobotsFileTokenType tokenType, int startIndex)
	{
		var value = Value.Slice(startIndex, Index - startIndex);
		var token = new RobotsFileToken(tokenType, value);
		return token;
	}

	private RobotsFileToken ReadComment()
	{
		var startIndex = Index;
		var newLineIndex = Value.Span
			.Slice(Index)
			.IndexOfAny('\r', '\n');

		if (newLineIndex == -1)
		{
			//If no new line is found, we skip to the end
			Index = Value.Length;
		}
		else
		{
			Index = newLineIndex;
		}

		return CreateToken(RobotsFileTokenType.Comment, startIndex);
	}

	private RobotsFileToken ReadWhitespace()
	{
		var startIndex = Index;
		while (true)
		{
			ReadNext();
			switch (Current)
			{
				case ' ':
				case '\t':
					ReadNext();
					continue;
				default:
					return CreateToken(RobotsFileTokenType.Whitespace, startIndex);
			}
		}
	}

	private RobotsFileToken ReadNewLine()
	{
		var startIndex = Index;
		if (Current == '\r' && Peek() == '\n')
		{
			ReadNext();
		}
		ReadNext();
		return CreateToken(RobotsFileTokenType.NewLine, startIndex);
	}

	private RobotsFileToken ReadDelimiter()
	{
		ReadNext();
		return CreateToken(RobotsFileTokenType.Delimiter, Index - 1);
	}

	/// <summary>
	/// Read the next characters in NoRobots RFC "value" syntax.
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// value		= &lt;any CHAR except CR or LF or "#"&gt;
	/// CHAR		= &lt;any US-ASCII character (octets 0 - 127)&gt;
	/// </code>
	/// </remarks>
	/// <returns></returns>
	private RobotsFileToken ReadValue()
	{
		const char ValidCharMax = (char)127;

		var startIndex = Index;
		while (true)
		{
			switch (Current)
			{
				case EndOfFile:
				case '#':
				case '\r':
				case '\n':
				case > ValidCharMax:
					return CreateToken(RobotsFileTokenType.Value, startIndex);
			}

			ReadNext();
		}
	}

	/// <summary>
	/// Like <see cref="ReadValue"/> but is always an invalid token type.
	/// </summary>
	/// <param name="startIndex"></param>
	/// <returns></returns>
	private RobotsFileToken ReadInvalidValue(int startIndex)
	{
		const char ValidCharMax = (char)127;

		while (true)
		{
			switch (Current)
			{
				case EndOfFile:
				case '#':
				case '\r':
				case '\n':
				case > ValidCharMax:
					return CreateToken(RobotsFileTokenType.Invalid, startIndex);
			}

			ReadNext();
		}
	}

	/// <summary>
	/// Read the next characters in NoRobots RFC "token" syntax.
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// token		= 1*&lt;any CHAR except CTLs or tspecials&gt;
	/// CHAR		= &lt;any US-ASCII character (octets 0 - 127)&gt;
	/// CTL		= &lt;any US-ASCII control character (octets 0 - 31) and DEL (127)&gt;
	/// tspecials	= "(" | ")" | "&lt;" | "&gt;" | "@"
	///			| "," | ";" | ":" | "\" | &lt;"&gt;
	///			| "/" | "[" | "]" | "?" | "="
	///			| "{" | "}" | SP | HT
	///	</code>
	/// </remarks>
	/// <returns></returns>
	private RobotsFileToken ReadValueInTokenFormat()
	{
		//The first and last non-CTL CHAR values
		const char ValidCharMin = (char)32;
		const char ValidCharMax = (char)126;

		var startIndex = Index;
		while (true)
		{
			switch (Current)
			{
				case EndOfFile:
				case < ValidCharMin:
				case > ValidCharMax:
				case var _ when NoRobotsRfcHelper.IsTSpecial(Current):
					//As part of "token" - must have at least one "CHAR"
					if (Index - startIndex > 0)
					{
						return CreateToken(RobotsFileTokenType.Value, startIndex);
					}
					return ReadInvalidValue(startIndex);
			}

			ReadNext();
		}
	}

	/// <summary>
	/// Read the next characters in NoRobots RFC "path" (or "rpath") syntax.
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
	/// <returns></returns>
	private RobotsFileToken ReadValueInPathFormat(bool enforceRPath)
	{
		var startIndex = Index;

		//As part of "rpath" - must have a leading slash
		if (enforceRPath)
		{
			if (Current != '/')
			{
				return ReadInvalidValue(startIndex);
			}

			ReadNext();
		}

		//As part of "fsegment" - must have at least one "pchar"
		if (NoRobotsRfcHelper.IsPChar(Value.Span.Slice(Index), out var isFSegmentEscape))
		{
			//Escape sequences are 3 characters so we need to skip them
			if (isFSegmentEscape)
			{
				Index += 3;
			}
			else
			{
				ReadNext();
			}
		}
		else
		{
			return ReadInvalidValue(startIndex);
		}

		while (true)
		{
			if (Current == '/')
			{
				continue;
			}
			else if (NoRobotsRfcHelper.IsPChar(Value.Span.Slice(Index), out var isEscapeSequence))
			{
				//Escape sequences are 3 characters so we need to skip them
				if (isEscapeSequence)
				{
					Index += 3;
					continue;
				}

				ReadNext();
			}

			break;
		}

		return CreateToken(RobotsFileTokenType.Value, startIndex);
	}
}

/// <summary>
/// Specifies the format required for a token value.
/// </summary>
public enum RobotsFileTokenValueFormat
{
	/// <summary>
	/// Enforce the format to match the NoRobots RFC "value" syntax.
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// value		= &lt;any CHAR except CR or LF or "#"&gt;
	/// CHAR		= &lt;any US-ASCII character (octets 0 - 127)&gt;
	/// </code>
	/// </remarks>
	Value,
	/// <summary>
	/// Enforce the format to match the NoRobots RFC "token" syntax.
	/// The values "User-agent", "Disallow" etc are examples of a "token".
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
	/// token		= 1*&lt;any CHAR except CTLs or tspecials&gt;
	/// CHAR		= &lt;any US-ASCII character (octets 0 - 127)&gt;
	/// CTL		= &lt;any US-ASCII control character (octets 0 - 31) and DEL (127)&gt;
	/// tspecials	= "(" | ")" | "&lt;" | "&gt;" | "@"
	///			| "," | ";" | ":" | "\" | &lt;"&gt;
	///			| "/" | "[" | "]" | "?" | "="
	///			| "{" | "}" | SP | HT
	///	</code>
	/// </remarks>
	Token,
	/// <summary>
	/// Enforce the format to match the NoRobots RFC "path" syntax.
	/// </summary>
	/// <remarks>
	/// <b>NoRobots RFC</b>
	/// <code>
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
	Path,
	/// <summary>
	/// Enforce the format to match the NoRobots RFC "rpath" syntax.
	/// This is the same as "path" syntax but with a leading slash.
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
	RPath
}