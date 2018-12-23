using RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotsExclusionTools
{
	public interface IRobotsEntryTokenParser
	{
		IEnumerable<SiteAccessEntry> GetSiteAccessEntries(IEnumerable<Token> tokens);
		IEnumerable<SitemapUrlEntry> GetSitemapUrlEntries(IEnumerable<Token> tokens);
	}
}
