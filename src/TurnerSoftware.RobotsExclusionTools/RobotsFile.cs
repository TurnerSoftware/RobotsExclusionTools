using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsFile
	{
		public IEnumerable<SiteAccessEntry> SiteAccessEntries { get; set; }
		public IEnumerable<SitemapUrlEntry> SitemapEntries { get; set; }
	}
}
