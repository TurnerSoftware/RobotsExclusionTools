using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class TokenMatch
	{
		public bool IsMatch { get; set; }
		public TokenType TokenType { get; set; }
		public string Value { get; set; }
		public int MatchLength { get; set; }
	}
}
