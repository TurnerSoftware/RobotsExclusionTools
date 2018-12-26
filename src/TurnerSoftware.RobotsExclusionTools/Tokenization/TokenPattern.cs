using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class TokenPattern
	{
		public TokenType Token { get; }
		public TokenType[] Preceeding { get; }
		public TokenType[] Succeeding { get; }

		public TokenPattern(TokenType token, TokenType[] preceeding, TokenType[] succeeding)
		{
			Token = token;
			Preceeding = preceeding ?? new TokenType[0];
			Succeeding = succeeding ?? new TokenType[0];
		}
	}
}
