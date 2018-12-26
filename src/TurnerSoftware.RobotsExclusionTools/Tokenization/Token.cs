using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class Token
	{
		public TokenType TokenType { get; }
		public string Value { get; }

		public static readonly Token NewLineToken = new Token(TokenType.NewLine, "\n");

		public Token(TokenType tokenType, string value)
		{
			TokenType = tokenType;
			Value = value;
		}
	}
}
