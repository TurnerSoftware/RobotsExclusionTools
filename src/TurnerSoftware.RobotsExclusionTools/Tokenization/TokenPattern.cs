using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class TokenPattern
	{
		public TokenType Token { get; }
		public IEnumerable<TokenType> Preceeding { get; }
		public IEnumerable<TokenType> Succeeding { get; }

		public TokenPattern(TokenType token, IEnumerable<TokenType> preceeding, IEnumerable<TokenType> succeeding)
		{
			Token = token;
			Preceeding = preceeding ?? Enumerable.Empty<TokenType>();
			Succeeding = succeeding ?? Enumerable.Empty<TokenType>();
		}
	}
}
