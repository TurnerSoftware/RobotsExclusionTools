using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class TokenPattern
	{
		public TokenType Token { get; }
		public IReadOnlyCollection<TokenType> Preceeding { get; }
		public IReadOnlyCollection<TokenType> Succeeding { get; }

		public TokenPattern(TokenType token, TokenType[] preceeding, TokenType[] succeeding)
		{
			Token = token;
			Preceeding = preceeding ?? Array.Empty<TokenType>();
			Succeeding = succeeding ?? Array.Empty<TokenType>();
		}
	}
}
