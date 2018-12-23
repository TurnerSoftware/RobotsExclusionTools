using System;
using System.Collections.Generic;
using System.Text;

namespace RobotsExclusionTools
{
	public class SiteAccessEntry
	{
		public IEnumerable<string> UserAgents { get; set; }
		public IEnumerable<string> Disallow { get; set; }

		public IEnumerable<string> Allow { get; set; }
		public int? CrawlDelay { get; set; }
	}
}
