using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

namespace TurnerSoftware.RobotsExclusionTools;

/// <summary>
/// Helps interpret the Robots directives from HTTP Header and HTML meta tags.
///	<para>
///	As there is no official RFC for parsing, the intepretation of the rules to follow
///	is based on Google's documentation for the Robots meta tag.
/// </para>
/// <para>
/// Google's documentation: <see href="https://developers.google.com/search/docs/advanced/robots/robots_meta_tag"/>
/// </para>
/// </summary>
/// <remarks>
/// This class won't perform requests on your behalf or pull meta tags or the headers from HTTP requests.
/// Instead, if you feed it a list of strings from the appropriate meta tags and headers, it will parse the rules for you.
/// </remarks>
public class RobotsPageParser : IRobotsPageDefinitionParser
{	
	public RobotsPageDefinition FromRules(IEnumerable<string> rules)
	{
		Parse(rules, out var pageAccessEntries);
		return new RobotsPageDefinition(pageAccessEntries);
	}

	private enum ParseState
	{
		UserAgentOrValue,
		Value
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ReadTillNextDirective(ref RobotsPageTokenReader reader)
	{
		while (reader.NextToken(out var token) && token.TokenType != RobotsPageTokenType.DirectiveDelimiter);
	}

	private void Parse(IEnumerable<string> rules, out IReadOnlyCollection<PageAccessEntry> pageAccessEntries)
	{
		var tmpEntries = new List<PageAccessEntry>();
		foreach (var rule in rules)
		{
			if (TryParse(rule.AsMemory(), out var entry))
			{
				tmpEntries.Add(entry);
			}
		}

		static IReadOnlyCollection<PageAccessDirective> Distinct(IEnumerable<PageAccessDirective> values)
		{
			//Squash directives with the same name together, prefering the last value
			return values
				.GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
				.Select(d => d.LastOrDefault())
				.ToArray();
		}

		var globalDirectives = tmpEntries
			.Where(e => e.UserAgent == Constants.UserAgentField)
			.SelectMany(e => e.Directives)
			.ToArray();

		pageAccessEntries = tmpEntries
			.GroupBy(e => e.UserAgent, StringComparer.OrdinalIgnoreCase)
			.Select(g => new PageAccessEntry(
				g.Key,
				Distinct(g.Key == Constants.UserAgentWildcard ?
					g.SelectMany(e => e.Directives) :
					//We want to preference the last value but not ignoring the global directives
					globalDirectives.Concat(g.SelectMany(e => e.Directives))
				)
			))
			.ToArray();
	}

	private bool TryParse(ReadOnlyMemory<char> value, out PageAccessEntry pageAccessEntry)
	{
		var parseState = ParseState.UserAgentOrValue;
		var reader = new RobotsPageTokenReader(value);

		var userAgent = Constants.UserAgentWildcard;
		var directives = new List<PageAccessDirective>();

		while (reader.NextToken(out var token))
		{
			switch (token.TokenType)
			{
				case RobotsPageTokenType.Value:
					var tokenValue = token.Value.Span.ToString();

					var directiveType = RobotsPageDirectives.GetDirectiveType(tokenValue);
					switch (directiveType)
					{
						case RobotsPageDirectives.DirectiveType.ValueOnly:
							foreach (var directive in directives)
							{
								if (directive.Name.Equals(tokenValue, StringComparison.OrdinalIgnoreCase))
								{
									//No point adding the same value twice
									goto ValueOnlyEnd;
								}
							}
							directives.Add(new PageAccessDirective(tokenValue));
							ValueOnlyEnd:
							continue;
						case RobotsPageDirectives.DirectiveType.FieldWithValue:
							//Expect the next token to be a field value delimiter
							if (!reader.NextToken(out token) || token.TokenType != RobotsPageTokenType.FieldValueDelimiter)
							{
								ReadTillNextDirective(ref reader);
								continue;
							}

							//Skip any whitespace that might exist
							if (token.TokenType == RobotsPageTokenType.Whitespace && !reader.NextToken(out token))
							{
								//Reached the end of the line
								continue;
							}

							if (token.TokenType == RobotsPageTokenType.Value)
							{
								var valueTokenString = token.Value.Span.ToString();
								for (var i = 0; i < directives.Count; i++)
								{
									var directive = directives[i];
									if (directive.Name.Equals(tokenValue, StringComparison.OrdinalIgnoreCase))
									{
										//Override any existing value for the directive
										directives[i] = new PageAccessDirective(tokenValue, valueTokenString);
										goto FieldWithValueEnd;
									}
								}
								directives.Add(new PageAccessDirective(tokenValue, valueTokenString));
							}
							FieldWithValueEnd:
							continue;
						default:
							//Assume this is a user agent but only do this once
							if (parseState != ParseState.UserAgentOrValue)
							{
								continue;
							}

							parseState = ParseState.Value;
							userAgent = tokenValue;
							continue;
					}
				case RobotsPageTokenType.Whitespace:
				case RobotsPageTokenType.DirectiveDelimiter:
					continue;
				case RobotsPageTokenType.FieldValueDelimiter:
					//Unexpected token type - read till the next directive delimiter
					ReadTillNextDirective(ref reader);
					continue;
			}
		}

		if (directives.Count > 0)
		{
			pageAccessEntry = new PageAccessEntry(userAgent, directives);
			return true;
		}

		pageAccessEntry = default;
		return false;
	}
}
