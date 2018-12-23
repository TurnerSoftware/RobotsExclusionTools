using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RobotsExclusionTools.Tokenization
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

		public TokenMatch Match(string input)
		{
			var match = Regex.Match(input);
			if (match.Success)
			{
				return new TokenMatch
				{
					IsMatch = true,
					RemainingText = input.Substring(match.Length),
					TokenType = TokenType,
					Value = match.Value
				};
			}
			else
			{
				return new TokenMatch { IsMatch = false };
			}
		}
	}
}
