using System;
using System.Collections.Generic;

namespace TurnerSoftware.RobotsExclusionTools;

public readonly record struct PageAccessEntry
{
	public string UserAgent { get; }	
	public IReadOnlyCollection<PageAccessDirective> Directives { get; }

	public PageAccessEntry(string userAgent, IReadOnlyCollection<PageAccessDirective> directives)
	{
		UserAgent = userAgent;
		Directives = directives;
	}

	public bool HasRule(ReadOnlySpan<char> directiveName) => TryGetValue(directiveName, out _);

	public bool TryGetValue(ReadOnlySpan<char> directiveName, out string value)
	{
		foreach (var directive in Directives)
		{
			if (directiveName.Equals(directive.Name.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				value = directive.Value;
				return true;
			}
		}
		value = null;
		return false;
	}
}

public readonly record struct PageAccessDirective(string Name, string Value)
{
	public PageAccessDirective(string name) : this(name, null) { }
}
