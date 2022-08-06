using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Helpers;

#if NET6_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Numerics;
#endif

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Index = {Index}; Current = {Current}")]
public struct RobotsFileTokenReader
{
	private const byte EndOfFile = 0x00;

	private readonly ReadOnlyMemory<byte> Value;
	private readonly bool IsSingleLine;
	private int Index;

	public RobotsFileTokenReader(ReadOnlyMemory<byte> value, bool isSingleLine = false)
	{
		Value = value;
		Index = 0;
		IsSingleLine = isSingleLine;
	}

	private byte Current
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

	private byte Peek()
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
			(byte)' ' or (byte)'\t' => ReadWhitespace(),
			(byte)'#' => ReadComment(),
			(byte)'\r' or (byte)'\n' => ReadNewLine(),
			(byte)':' => ReadDelimiter(),
			_ => ReadValue(valueFormat)
		};
		return true;
	}

	public void SkipLine()
	{
		if (IsSingleLine)
		{
			Index = Value.Length;
		}
		else if (Index < Value.Length)
		{
			var newLineIndex = Value.Span
				.Slice(Index)
				.IndexOfAny((byte)'\r', (byte)'\n');

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
				case (byte)' ':
				case (byte)'\t':
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
			.IndexOfAny((byte)'\r', (byte)'\n');

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
	private unsafe RobotsFileToken ReadValue(RobotsFileTokenValueFormat valueFormat)
	{
		var startIndex = Index;
#if NET6_0_OR_GREATER
		if (Avx2.IsSupported)
		{
			fixed (byte* valuePtr = &Value.Span.GetPinnableReference())
			{
				var lowerBoundsExclusive = Vector256.Create((byte)0x20).AsSByte();
				var commentHash = Vector256.Create((sbyte)'#');
				var delimiter = Vector256.Create((sbyte)':');

				var searchLength = Value.Length - Index;
				while (searchLength > 0)
				{
					var valueVector = Avx.LoadDquVector256(valuePtr + Index).AsSByte();
					var lowerBoundsCheck = Avx2.CompareGreaterThan(valueVector, lowerBoundsExclusive);
					var commentHashCheck = Avx2.AndNot(
						Avx2.CompareEqual(valueVector, commentHash),
						Vector256<sbyte>.AllBitsSet
					);

					var allowedCharacters = Avx2.And(lowerBoundsCheck, commentHashCheck);

					if (valueFormat is RobotsFileTokenValueFormat.RuleName)
					{
						allowedCharacters = Avx2.And(
							allowedCharacters,
							Avx2.AndNot(
								Avx2.CompareEqual(valueVector, delimiter),
								Vector256<sbyte>.AllBitsSet
							)
						);
					}

					var match = (uint)Avx2.MoveMask(
						allowedCharacters.AsByte()
					);

					if (searchLength < Vector256<byte>.Count)
					{
						//This zeros-out the bits outside of the search area as a protection to over-reading
						var matchCutoff = ~(uint.MaxValue << searchLength);
						match &= matchCutoff;
					}

					if (match == uint.MaxValue)
					{
						Index += Vector256<byte>.Count;
						searchLength -= Vector256<byte>.Count;
						continue;
					}

					Index += BitOperations.TrailingZeroCount(match ^ uint.MaxValue) / sizeof(byte);
					//Fallback to slow processing for any remainder
					break;
				}
			}
		}
#endif
		return ReadValueSlow(startIndex, valueFormat);
	}

	private RobotsFileToken ReadValueSlow(int startIndex, RobotsFileTokenValueFormat valueFormat)
	{
		const byte UTF8_1_NoCtl_Low = 0x21;

		while (true)
		{
			//This kinda needs to stop at `:` but not all the time
			switch (Current)
			{
				case EndOfFile:
				case (byte)' ':
				case (byte)'\t':
				case (byte)'\r':
				case (byte)'\n':
				case (byte)'#':
				case (byte)':' when valueFormat is RobotsFileTokenValueFormat.RuleName:
					return CreateToken(RobotsFileTokenType.Value, startIndex);
				case < UTF8_1_NoCtl_Low:
					return ReadInvalidValue(startIndex);
				default:
					if (RobotsExclusionProtocolHelper.TryReadUtf8ByteSequence(Value.Span.Slice(Index), out var numberOfBytes))
					{
						Index += numberOfBytes;
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
	/// The token ends when either a comment, new line or end of data is found.
	/// </summary>
	/// <param name="startIndex"></param>
	/// <returns></returns>
	private RobotsFileToken ReadInvalidValue(int startIndex)
	{
		var invalidTokenEndIndex = Value.Span
			.Slice(Index)
			.IndexOfAny((byte)'#', (byte)'\r', (byte)'\n');

		if (invalidTokenEndIndex == -1)
		{
			//If none of the expected characters are found, we skip to the end
			Index = Value.Length;
		}
		else
		{
			Index += invalidTokenEndIndex;
		}

		return CreateToken(RobotsFileTokenType.Invalid, startIndex);
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
