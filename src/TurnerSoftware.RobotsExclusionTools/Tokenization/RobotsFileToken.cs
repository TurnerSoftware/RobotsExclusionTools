using System;
using System.Diagnostics;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

[DebuggerDisplay("Type = {TokenType}; Value = {ToString()}")]
public readonly record struct RobotsFileToken
{
	public readonly RobotsFileTokenType TokenType;
	public readonly ReadOnlyMemory<byte> Value;

	public RobotsFileToken(RobotsFileTokenType tokenType, ReadOnlyMemory<byte> value)
	{
		TokenType = tokenType;
		Value = value;
	}

	/// <summary>
	/// Returns the token value encoded as a string.
	/// </summary>
	/// <returns>The token value.</returns>
	public override string ToString()
	{
#if NETSTANDARD2_0
		return Encoding.UTF8.GetString(Value.ToArray());
#else
		return Encoding.UTF8.GetString(Value.Span);
#endif
	}
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