using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class Token
	{
		public TokenType TokenType { get; }
		public string Value { get; }

		public Token(TokenType tokenType, string value)
		{
			TokenType = tokenType;
			Value = value;
		}
	}
}
