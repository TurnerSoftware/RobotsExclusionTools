using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class TokenDefinition
	{
		private Regex Regex { get; }
		private TokenType TokenType { get; }

		public TokenDefinition(TokenType tokenType, string regex)
		{
			Regex = new Regex(regex, RegexOptions.IgnoreCase);
			TokenType = tokenType;
		}

		public TokenMatch Match(string input, int offset = 0)
		{
			var match = Regex.Match(input, offset);
			if (match.Success)
			{
				return new TokenMatch
				{
					IsMatch = true,
					MatchLength = match.Length,
					TokenType = TokenType,
					Value = match.Value
				};
			}
			else
			{
				return TokenMatch.NoMatch;
			}
		}
	}
}
