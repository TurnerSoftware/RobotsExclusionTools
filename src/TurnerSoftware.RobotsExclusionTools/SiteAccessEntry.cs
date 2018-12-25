using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class SiteAccessEntry
	{
		public IEnumerable<string> UserAgents { get; set; }
		public IEnumerable<SiteAccessPathRule> PathRules { get; set; }
		public int? CrawlDelay { get; set; }
	}

	public class SiteAccessPathRule
	{
		public string Path { get; set; }
		public PathRuleType RuleType { get; set; }
	}

	public enum PathRuleType
	{
		Allow,
		Disallow
	}
}
