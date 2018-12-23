using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public interface ITokenizer
	{
		IEnumerable<Token> Tokenize(string text);
	}
}
