using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsPageDefinition
	{
		public IEnumerable<PageAccessEntry> PageAccessEntries { get; set; } = Enumerable.Empty<PageAccessEntry>();
		
		public bool CanIndex(string userAgent)
		{
			var entry = GetEntryForUserAgent(userAgent);
			if (entry != null)
			{
				var disallowIndex = entry.Rules.Any(r =>
					r.RuleName.Equals("noindex", StringComparison.InvariantCultureIgnoreCase) ||
					r.RuleName.Equals("none", StringComparison.InvariantCultureIgnoreCase)
				);
				return !disallowIndex;
			}
			return true;
		}
		
		public bool CanFollowLinks(string userAgent)
		{
			var entry = GetEntryForUserAgent(userAgent);
			if (entry != null)
			{
				var disallowFollow = entry.Rules.Any(r =>
					r.RuleName.Equals("nofollow", StringComparison.InvariantCultureIgnoreCase) ||
					r.RuleName.Equals("none", StringComparison.InvariantCultureIgnoreCase)
				);
				return !disallowFollow;
			}
			return true;
		}
		
		public PageAccessEntry GetEntryForUserAgent(string userAgent)
		{
			PageAccessEntry globalEntry = null;

			foreach (var pageAccessEntry in PageAccessEntries)
			{
				if (globalEntry == null && pageAccessEntry.UserAgent == "*")
				{
					globalEntry = pageAccessEntry;
				}

				if (userAgent.IndexOf(pageAccessEntry.UserAgent) != -1)
				{
					return pageAccessEntry;
				}
			}

			return globalEntry;
		}
	}
}
