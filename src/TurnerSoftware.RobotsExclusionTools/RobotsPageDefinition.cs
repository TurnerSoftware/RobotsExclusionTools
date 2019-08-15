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
			return Can("index", userAgent);
		}
		
		public bool CanFollowLinks(string userAgent)
		{
			return Can("follow", userAgent);
		}
		
		public bool Can(string ruleName, string userAgent)
		{
			var negatedRule = "no" + ruleName;
			var entry = GetEntryForUserAgent(userAgent);
			if (entry != null)
			{
				var items = entry.Rules.ToArray();
				
				//Going through the array backwards gives priority to more specific values (useragent values over global values)
				for (var i = items.Length - 1; i >= 0; i--)
				{
					var rule = items[i];
					if (rule.RuleName.Equals("all", StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
					else if (rule.RuleName.Equals(negatedRule, StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
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

				if (userAgent.IndexOf(pageAccessEntry.UserAgent, StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					return pageAccessEntry;
				}
			}

			return globalEntry;
		}
	}
}
