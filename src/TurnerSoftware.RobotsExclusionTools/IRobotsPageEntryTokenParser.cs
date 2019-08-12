using System;
using System.Collections.Generic;
using System.Text;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

namespace TurnerSoftware.RobotsExclusionTools
{
	public interface IRobotsPageEntryTokenParser
	{
		IEnumerable<PageAccessEntry> GetPageAccessEntries(IEnumerable<Token> tokens);
	}
}
