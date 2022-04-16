using System;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public readonly struct RobotsFileToken
{
	public readonly RobotsFileTokenType TokenType;
	public readonly ReadOnlyMemory<char> Value;

	public RobotsFileToken(RobotsFileTokenType tokenType, ReadOnlyMemory<char> value)
	{
		TokenType = tokenType;
		Value = value;
	}
}

public enum RobotsFileTokenType
{
	Value,
	Comment,
	Delimiter,
	NewLine,
	Whitespace
}
