using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TurnerSoftware.RobotsExclusionTools;

[DebuggerDisplay("UserAgents = {UserAgents.Count}; PathRules = {PathRules.Count}; CrawlDelay = {CrawlDelay}")]
public readonly record struct SiteAccessEntry(
	IReadOnlyCollection<string> UserAgents,
	IReadOnlyCollection<SiteAccessPathRule> PathRules,
	int? CrawlDelay
);

[DebuggerDisplay("{RuleType} {Path}")]
public readonly record struct SiteAccessPathRule(
	string Path, 
	PathRuleType RuleType
);

public enum PathRuleType
{
	Allow,
	Disallow
}

[DebuggerDisplay("{Sitemap}")]
public readonly record struct SitemapUrlEntry(Uri Sitemap);