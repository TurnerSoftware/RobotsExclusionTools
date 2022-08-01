using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Index = {Index}; Current = {Current}")]
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
			_ => ReadValue(valueFormat)
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
				Index += newLineIndex;
				if (Current == '\r' && Peek() == '\n')
				{
					ReadNext();
				}
			}
		}
	}

	private RobotsFileToken CreateToken(RobotsFileTokenType tokenType, int startIndex)
	{
		var value = Value.Slice(startIndex, Index - startIndex);
		var token = new RobotsFileToken(tokenType, value);
		return token;
	}

	/// <summary>
	/// Read the next characters in Robots Exclusion Protocol "WS" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// WS	= %x20 / %x09
	/// </code>
	/// </remarks>
	/// <returns></returns>
	private RobotsFileToken ReadWhitespace()
	{
		var startIndex = Index;
		ReadNext();
		while (true)
		{
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

	/// <summary>
	/// Read the next characters in Robots Exclusion Protocol "comment" syntax.
	/// Current implementation ignores UTF-8 character requirement.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// comment		= "#" *(UTF8-char-noctl / WS / "#")
	/// WS			= %x20 / %x09
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
			Index += newLineIndex;
		}

		return CreateToken(RobotsFileTokenType.Comment, startIndex);
	}

	/// <summary>
	/// Read the next characters in Robots Exclusion Protocol "NL" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// NL	= %x0D / %x0A / %x0D.0A
	/// </code>
	/// </remarks>
	/// <returns></returns>
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
	/// Read the next characters in Robots Exclusion Protocol "UTF8-char-noctl" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
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
	private RobotsFileToken ReadValue(RobotsFileTokenValueFormat valueFormat)
	{
		const char UTF8_1_NoCtl_Low = (char)0x21;

		var startIndex = Index;
		while (true)
		{
			//This kinda needs to stop at `:` but not all the time
			switch (Current)
			{
				case EndOfFile:
				case < UTF8_1_NoCtl_Low:
				case '#':
				case ':' when valueFormat is RobotsFileTokenValueFormat.RuleName:
					return CreateToken(RobotsFileTokenType.Value, startIndex);
				default:
					if (RobotsExclusionProtocolHelper.TryReadUtf8ByteSequence(Value.Span.Slice(Index), out var numberOfBytes))
					{
						ReadNext();
						if (numberOfBytes > 2)
						{
							ReadNext();
						}
						continue;
					}
					else
					{
						return ReadInvalidValue(startIndex);
					}
			}
		}
	}

	/// <summary>
	/// Like <see cref="ReadValue"/> but is always an invalid token type.
	/// </summary>
	/// <param name="startIndex"></param>
	/// <returns></returns>
	private RobotsFileToken ReadInvalidValue(int startIndex)
	{
		while (true)
		{
			switch (Current)
			{
				case EndOfFile:
				case '#':
				case '\r':
				case '\n':
					return CreateToken(RobotsFileTokenType.Invalid, startIndex);
			}

			ReadNext();
		}
	}
}

public enum RobotsFileTokenValueFormat
{
	/// <summary>
	/// Follow the Robots Exclusion Protocol "UTF8-char-noctl" syntax.
	/// </summary>
	/// <remarks>
	/// <b>Robots Exclusion Protocol</b>
	/// <code>
	/// UTF8-char-noctl	= UTF8-1-noctl / UTF8-2 / UTF8-3 / UTF8-4
	/// UTF8-1-noctl		= %x21 / %x22 / %x24-7F ; excluding control, space, '#'
	/// UTF8-2			= %xC2-DF UTF8-tail
	/// UTF8-3			= %xE0 %xA0-BF UTF8-tail / %xE1-EC 2UTF8-tail /
	///					%xED %x80-9F UTF8-tail / %xEE-EF 2UTF8-tail
	/// UTF8-4			= %xF0 %x90-BF 2UTF8-tail / %xF1-F3 3UTF8-tail /
	///					%xF4 %x80-8F 2UTF8-tail
	/// </code>
	/// </remarks>
	Value,
	/// <summary>
	/// Same as <see cref="Value"/> but additionally will stop at the first delimiter (<c>:</c>) found.
	/// </summary>
	RuleName
}
