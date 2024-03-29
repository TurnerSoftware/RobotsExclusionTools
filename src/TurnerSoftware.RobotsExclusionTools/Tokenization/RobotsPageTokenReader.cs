﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Index = {Index}; Current = {Current}")]
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

	public bool NextToken(out RobotsPageToken token, RobotsPageTokenValueFormat valueFormat = RobotsPageTokenValueFormat.Strict)
	{
		if (Current == EndOfLine)
		{
			token = default;
			return false;
		}

		token = Current switch
		{
			' ' or '\t' or '\r' or '\n' => ReadWhitespace(),
			':' => ReadChar(RobotsPageTokenType.FieldValueDelimiter),
			',' => ReadChar(RobotsPageTokenType.DirectiveDelimiter),
			_ => ReadValue(valueFormat)
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
				case '\r':
				case '\n':
					ReadNext();
					continue;
				default:
					return CreateToken(RobotsPageTokenType.Whitespace, startIndex);
			}
		}
	}

	private RobotsPageToken ReadValue(RobotsPageTokenValueFormat valueFormat)
	{
		var startIndex = Index;

		if (valueFormat is RobotsPageTokenValueFormat.Strict)
		{
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
					case '\r':
					case '\n':
						return CreateToken(RobotsPageTokenType.Value, startIndex);
				}
			}
		}
		else
		{
			while (true)
			{
				ReadNext();
				switch (Current)
				{
					case EndOfLine:
					case ',':
					case '\r':
					case '\n':
						//Walk back any spaces we may have encountered.
						//This will effectively trim the value of spaces.
						while (Index > 0 && Value.Span[Index - 1] == ' ')
						{
							Index--;
						}
						return CreateToken(RobotsPageTokenType.Value, startIndex);
				}
			}
		}
	}
}

public enum RobotsPageTokenValueFormat
{
	/// <summary>
	/// For processing tokens without accepting a colon (:), comma (,), 
	/// space ( ), horizontal tab (\t), carriage return (\r) or new line (\n).
	/// </summary>
	Strict,
	/// <summary>
	/// For processing tokens without accepting a comma (,), carriage return (\r) 
	/// or new line (\n). Any trailing spaces on the value will be ignored.
	/// </summary>
	Flexible
}