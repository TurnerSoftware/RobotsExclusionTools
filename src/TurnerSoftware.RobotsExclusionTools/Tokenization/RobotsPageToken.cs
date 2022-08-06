using System;
using System.Diagnostics;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Type = {TokenType}; Value = {ToString()}")]
public readonly record struct RobotsPageToken
{
	public readonly RobotsPageTokenType TokenType;
	public readonly ReadOnlyMemory<char> Value;

	public RobotsPageToken(RobotsPageTokenType tokenType, ReadOnlyMemory<char> value)
	{
		TokenType = tokenType;
		Value = value;
	}

	public override string ToString()
	{
		return Value.Span.ToString();
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
