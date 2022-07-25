using System;
using System.Collections.Generic;
using System.Linq;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools;

public record class RobotsFile
{
	public Uri BaseUri { get; }
	public IReadOnlyCollection<SiteAccessEntry> SiteAccessEntries { get; }
	public IReadOnlyCollection<SitemapUrlEntry> SitemapEntries { get; }

	private readonly static IReadOnlyCollection<SiteAccessEntry> DenyAllSiteAccessEntries = new[]
	{
		new SiteAccessEntry
		{
			UserAgents = new[] { "*" },
			PathRules = new[]
			{
				new SiteAccessPathRule
				{
					Path = "/",
					RuleType = PathRuleType.Disallow
				}
			}
		}
	};

	public RobotsFile(
		Uri baseUri, 
		IReadOnlyCollection<SiteAccessEntry> siteAccessEntries, 
		IReadOnlyCollection<SitemapUrlEntry> sitemapEntries
	)
	{
		BaseUri = baseUri;
		SiteAccessEntries = siteAccessEntries ?? Array.Empty<SiteAccessEntry>();
		SitemapEntries = sitemapEntries ?? Array.Empty<SitemapUrlEntry>();
	}

	internal static RobotsFile ConditionalRobots(Uri baseUri, bool condition) => 
		condition ? AllowAllRobots(baseUri) : DenyAllRobots(baseUri);

	public static RobotsFile AllowAllRobots(Uri baseUri) => new(baseUri, default, default);

	public static RobotsFile DenyAllRobots(Uri baseUri) => new(baseUri, DenyAllSiteAccessEntries, default);

	public bool IsAllowedAccess(Uri uri, string userAgent)
	{
		if (!uri.IsAbsoluteUri)
		{
			uri = new Uri(BaseUri, uri);
		}

		if (TryGetEntryForUserAgent(userAgent, out var siteAccessEntry))
		{
			return PathComparisonUtility.IsAllowed(siteAccessEntry, uri);
		}

		//If no entry is defined, the robot is allowed access by default
		return true;
	}

	public bool TryGetEntryForUserAgent(string userAgent, out SiteAccessEntry matchingAccessEntry)
	{
		SiteAccessEntry? globalEntry = default;

		foreach (var siteAccessEntry in SiteAccessEntries)
		{
			if (!globalEntry.HasValue && siteAccessEntry.UserAgents.Contains(Constants.UserAgentWildcard))
			{
				globalEntry = siteAccessEntry;
			}

			if (siteAccessEntry.UserAgents.Any(u => userAgent.IndexOf(u, StringComparison.InvariantCultureIgnoreCase) != -1))
			{
				matchingAccessEntry = siteAccessEntry;
				return true;
			}
		}

		matchingAccessEntry = globalEntry ?? default;
		return globalEntry.HasValue;
	}
}
