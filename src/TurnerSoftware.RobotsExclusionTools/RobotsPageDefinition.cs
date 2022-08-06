using System;
using System.Collections.Generic;

namespace TurnerSoftware.RobotsExclusionTools;

public record class RobotsPageDefinition
{
	public IReadOnlyCollection<PageAccessEntry> PageAccessEntries { get; }

	public RobotsPageDefinition(IReadOnlyCollection<PageAccessEntry> pageAccessEntries)
	{
		PageAccessEntries = pageAccessEntries;
	}

	public bool CanIndex(string userAgent) => CanIndex(userAgent.AsSpan());
	public bool CanIndex(ReadOnlySpan<char> userAgent)
	{
		if (TryGetEntryForUserAgent(userAgent, out var entry))
		{
			if (entry.HasRule(RobotsPageDirectives.NoIndex.AsSpan()) || entry.HasRule(RobotsPageDirectives.None.AsSpan()))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanFollowLinks(string userAgent) => CanFollowLinks(userAgent.AsSpan());
	public bool CanFollowLinks(ReadOnlySpan<char> userAgent)
	{
		if (TryGetEntryForUserAgent(userAgent, out var entry))
		{
			if (entry.HasRule(RobotsPageDirectives.NoFollow.AsSpan()) || entry.HasRule(RobotsPageDirectives.None.AsSpan()))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasRule(string ruleName, string userAgent) => HasRule(ruleName.AsSpan(), userAgent.AsSpan());
	public bool HasRule(ReadOnlySpan<char> ruleName, ReadOnlySpan<char> userAgent)
	{
		if (TryGetEntryForUserAgent(userAgent, out var entry))
		{
			return entry.HasRule(ruleName);
		}
		return false;
	}

	public bool TryGetRuleValue(string ruleName, string userAgent, out string value) => TryGetRuleValue(ruleName.AsSpan(), userAgent.AsSpan(), out value);
	public bool TryGetRuleValue(ReadOnlySpan<char> ruleName, ReadOnlySpan<char> userAgent, out string value)
	{
		if (TryGetEntryForUserAgent(userAgent, out var pageAccessEntry))
		{
			return pageAccessEntry.TryGetValue(ruleName, out value);
		}
		value = null;
		return false;
	}

	public bool TryGetEntryForUserAgent(ReadOnlySpan<char> userAgent, out PageAccessEntry pageAccessEntry)
	{
		PageAccessEntry globalEntry = default;
		foreach (var entry in PageAccessEntries)
		{
			if (globalEntry.UserAgent is null && entry.UserAgent == Constants.UserAgentWildcard)
			{
				globalEntry = entry;
			}

			if (userAgent.IndexOf(entry.UserAgent.AsSpan(), StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				pageAccessEntry = entry;
				return true;
			}
		}

		if (globalEntry.Directives?.Count > 0)
		{
			pageAccessEntry = globalEntry;
			return true;
		}

		pageAccessEntry = default;
		return false;
	}
}
