using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsFile
	{
		public Uri BaseUri { get; }

		public IEnumerable<SiteAccessEntry> SiteAccessEntries { get; set; } = Enumerable.Empty<SiteAccessEntry>();
		public IEnumerable<SitemapUrlEntry> SitemapEntries { get; set; } = Enumerable.Empty<SitemapUrlEntry>();

		public RobotsFile(Uri baseUri)
		{
			BaseUri = baseUri;
		}

		public static RobotsFile AllowAllRobots(Uri baseUri)
		{
			return new RobotsFile(baseUri);
		}

		public static RobotsFile DenyAllRobots(Uri baseUri)
		{
			return new RobotsFile(baseUri)
			{
				SiteAccessEntries = new []
				{
					new SiteAccessEntry
					{
						Disallow = new [] { "/" }
					}
				}
			};
		}

		public bool IsAllowedAccess(Uri uri, string userAgent)
		{
			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(BaseUri, uri);
			}

			var entry = GetEntryForUserAgent(userAgent);
			var pathComparisonUtility = new PathComparisonUtility();
			return pathComparisonUtility.IsAllowed(entry, uri);
		}

		public SiteAccessEntry GetEntryForUserAgent(string userAgent)
		{
			SiteAccessEntry globalEntry = null;

			foreach (var siteAccessEntry in SiteAccessEntries)
			{
				if (globalEntry == null && siteAccessEntry.UserAgents.Any(u => u == "*"))
				{
					globalEntry = siteAccessEntry;
				}

				if (siteAccessEntry.UserAgents.Any(u => userAgent.IndexOf(u, StringComparison.InvariantCultureIgnoreCase) != -1))
				{
					return siteAccessEntry;
				}
			}

			return globalEntry;
		}
	}
}
