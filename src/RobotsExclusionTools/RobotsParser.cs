using RobotsExclusionTools.Tokenization;
using System;

namespace RobotsExclusionTools
{
	public class RobotsParser : IRobotsParser
	{
		private ITokenizer Tokenizer { get; }

		public RobotsParser(ITokenizer tokenizer)
		{
			Tokenizer = tokenizer;
		}
	}
}
