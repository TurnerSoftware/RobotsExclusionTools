using System;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public readonly struct RobotsPageToken
{
	public readonly RobotsPageTokenType TokenType;
	public readonly ReadOnlyMemory<char> Value;

	public RobotsPageToken(RobotsPageTokenType tokenType, ReadOnlyMemory<char> value)
	{
		TokenType = tokenType;
		Value = value;
	}
}

public enum RobotsPageTokenType
{
	Value,
	/// <summary>
	/// A comma between directives. For example "<c>noindex,nofollow</c>".
	/// </summary>
	DirectiveDelimiter,
	/// <summary>
	/// A colon between a directive and its value. For example "<c>max-snippet:0</c>".
	/// This is also used between user agents and the rest of the directives.
	/// </summary>
	FieldValueDelimiter,
	Whitespace
}
