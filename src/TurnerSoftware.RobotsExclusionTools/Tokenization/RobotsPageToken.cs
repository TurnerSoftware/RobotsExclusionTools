using System;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public readonly record struct RobotsPageToken(RobotsPageTokenType TokenType, ReadOnlyMemory<char> Value);

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
