using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class SiteAccessEntry
	{
		public IEnumerable<string> UserAgents { get; set; }
		//TODO: Combine "Disallow" and "Allow" into same list via new class "AccessPath"?
		//		This allows the code to more accurately match the RFC in that
		//		the rules are run in the specific order they are found
		public IEnumerable<string> Disallow { get; set; }

		public IEnumerable<string> Allow { get; set; }
		public int? CrawlDelay { get; set; }
	}
}
