using System;
using System.Runtime.CompilerServices;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public struct RobotsPageTokenReader
{
	private const char EndOfLine = char.MinValue;

	private readonly ReadOnlyMemory<char> Value;
	private int Index;

	public RobotsPageTokenReader(ReadOnlyMemory<char> value)
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

			return EndOfLine;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ReadNext() => Index++;

	public bool NextToken(out RobotsPageToken token)
	{
		if (Current == EndOfLine)
		{
			token = default;
			return false;
		}

		token = Current switch
		{
			' ' or '\t' => ReadWhitespace(),
			':' => ReadChar(RobotsPageTokenType.FieldValueDelimiter),
			',' => ReadChar(RobotsPageTokenType.DirectiveDelimiter),
			_ => ReadValue()
		};
		return true;
	}

	private RobotsPageToken CreateToken(RobotsPageTokenType tokenType, int startIndex)
	{
		var value = Value.Slice(startIndex, Index - startIndex);
		var token = new RobotsPageToken(tokenType, value);
		return token;
	}

	private RobotsPageToken ReadChar(RobotsPageTokenType tokenType)
	{
		ReadNext();
		return CreateToken(tokenType, Index - 1);
	}

	private RobotsPageToken ReadWhitespace()
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
					return CreateToken(RobotsPageTokenType.Whitespace, startIndex);
			}
		}
	}

	private RobotsPageToken ReadValue()
	{
		var startIndex = Index;
		while (true)
		{
			ReadNext();
			switch (Current)
			{
				case EndOfLine:
				case ':':
				case ',':
				case ' ':
				case '\t':
					return CreateToken(RobotsPageTokenType.Value, startIndex);
			}
		}
	}
}
