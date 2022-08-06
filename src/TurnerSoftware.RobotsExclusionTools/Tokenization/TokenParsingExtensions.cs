using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization;

public static class TokenParsingExtensions
{
	public static bool IsValidProductToken(this RobotsFileToken token) => RobotsExclusionProtocolHelper.IsValidProductToken(token.Value.Span);
	public static bool IsValidPath(this RobotsFileToken token) => RobotsExclusionProtocolHelper.IsValidPath(token.Value.Span);
}
