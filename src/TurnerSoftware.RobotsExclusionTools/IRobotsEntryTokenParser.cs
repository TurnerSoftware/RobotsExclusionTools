using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public interface IRobotsEntryTokenParser
	{
		IEnumerable<SiteAccessEntry> GetSiteAccessEntries(IEnumerable<Token> tokens);
		IEnumerable<SitemapUrlEntry> GetSitemapUrlEntries(IEnumerable<Token> tokens);
	}
}
