using System;
using System.Collections.Generic;

namespace TurnerSoftware.RobotsExclusionTools;

public readonly record struct SiteAccessEntry(
	IReadOnlyCollection<string> UserAgents,
	IReadOnlyCollection<SiteAccessPathRule> PathRules,
	int? CrawlDelay
);

public readonly record struct SiteAccessPathRule(
	string Path, 
	PathRuleType RuleType
);

public enum PathRuleType
{
	Allow,
	Disallow
}

public readonly record struct SitemapUrlEntry(Uri Sitemap);