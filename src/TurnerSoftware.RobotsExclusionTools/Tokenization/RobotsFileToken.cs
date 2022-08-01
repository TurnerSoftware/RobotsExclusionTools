using System;
using System.Diagnostics;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Type = {TokenType}; Value = {Value}")]
public readonly struct RobotsFileToken
{
	public readonly RobotsFileTokenType TokenType;
	public readonly ReadOnlyMemory<char> Value;

	public RobotsFileToken(RobotsFileTokenType tokenType, ReadOnlyMemory<char> value)
	{
		TokenType = tokenType;
		Value = value;
	}

	/// <summary>
	/// Returns the token value.
	/// </summary>
	/// <returns>The token value.</returns>
	public override string ToString() => Value.Span.ToString();
}

public enum RobotsFileTokenType
{
	Invalid,
	Value,
	Comment,
	Delimiter,
	NewLine,
	Whitespace
}
