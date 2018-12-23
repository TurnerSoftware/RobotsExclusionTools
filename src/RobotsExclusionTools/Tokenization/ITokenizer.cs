using System;
using System.Collections.Generic;
using System.Text;

namespace RobotsExclusionTools.Tokenization
{
	public interface ITokenizer
	{
		IEnumerable<Token> Tokenize(string text);
	}
}
