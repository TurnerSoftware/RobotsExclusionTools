using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization.TokenParsers
{
	public interface IRobotsPageTokenParser
	{
		IEnumerable<PageAccessEntry> GetPageAccessEntries(IEnumerable<Token> tokens);
	}
}
