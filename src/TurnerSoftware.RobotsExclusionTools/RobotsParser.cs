using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.IO;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsParser : IRobotsParser
	{
		private ITokenizer Tokenizer { get; }
		private ITokenPatternValidator PatternValidator { get; }

		public RobotsParser() : this(new Tokenizer(), new TokenPatternValidator()) { }

		public RobotsParser(ITokenizer tokenizer, ITokenPatternValidator patternValidator)
		{
			Tokenizer = tokenizer;
			PatternValidator = patternValidator;
		}
		/*
		public RobotsFile FromString(string robotsText)
		{

		}

		public RobotsFile FromUri(Uri robotsUri)
		{

		}

		public RobotsFile FromStream(Stream stream)
		{

		}*/
	}
}
