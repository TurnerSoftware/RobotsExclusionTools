using System;
using System.Runtime.CompilerServices;

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

	public bool NextToken(out RobotsFileToken token)
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
			_ => ReadValue()
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

	private RobotsFileToken ReadValue()
	{
		var startIndex = Index;
		while (true)
		{
			ReadNext();
			switch (Current)
			{
				case EndOfFile:
				case '#':
				case '\r':
				case '\n':
					return CreateToken(RobotsFileTokenType.Value, startIndex);
			}
		}
	}
}
