﻿using System;
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

	private static void Parse(IEnumerable<string> rules, out IReadOnlyCollection<PageAccessEntry> pageAccessEntries)
	{
		var userAgentDirectives = new Dictionary<string, List<PageAccessDirective>>(StringComparer.OrdinalIgnoreCase);

		foreach (var rule in rules)
		{
			ParseLine(rule.AsMemory(), userAgentDirectives);
		}

		IList<PageAccessDirective> globalDirectives = Array.Empty<PageAccessDirective>();
		if (userAgentDirectives.TryGetValue(Constants.UserAgentWildcard, out var foundGlobalDirectives))
		{
			globalDirectives = foundGlobalDirectives;
		}

		static void CombineDirectives(List<PageAccessDirective> directives, IList<PageAccessDirective> globalDirectives)
		{
			for (var j = 0; j < globalDirectives.Count; j++)
			{
				var globalDirective = globalDirectives[j];

				for (var i = 0; i < directives.Count; i++)
				{
					var directive = directives[i];
					if (globalDirective.Name.Equals(directive.Name, StringComparison.OrdinalIgnoreCase))
					{
						goto NextGlobalDirective;
					}
				}

				directives.Add(globalDirective);
				NextGlobalDirective:
				;
			}
		}

		var entries = new List<PageAccessEntry>();
		foreach (var (userAgent, directives) in userAgentDirectives)
		{
			if (globalDirectives is not null)
			{
				CombineDirectives(directives, globalDirectives);
			}

			entries.Add(new PageAccessEntry(userAgent, directives));
		}

		pageAccessEntries = entries;
	}

	private static void ParseLine(ReadOnlyMemory<char> value, Dictionary<string, List<PageAccessDirective>> userAgentDirectives)
	{
		var parseState = ParseState.UserAgentOrValue;
		var reader = new RobotsPageTokenReader(value);

		var userAgent = Constants.UserAgentWildcard;
		static List<PageAccessDirective> GetDirectives(
			Dictionary<string, List<PageAccessDirective>> userAgentDirectives, 
			string userAgent
		)
		{
			if (!userAgentDirectives.TryGetValue(userAgent, out var directives))
			{
				directives = new List<PageAccessDirective>();
				userAgentDirectives[userAgent] = directives;
			}
			return directives;
		}

		while (reader.NextToken(out var token))
		{
			switch (token.TokenType)
			{
				case RobotsPageTokenType.Value:
					var tokenValue = token.ToString();

					var directiveType = RobotsPageDirectives.GetDirectiveType(tokenValue);
					switch (directiveType)
					{
						case RobotsPageDirectives.DirectiveType.ValueOnly:
							{
								var directives = GetDirectives(userAgentDirectives, userAgent);
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
							}
						case RobotsPageDirectives.DirectiveType.FieldWithValue:
							{
								//Expect the next token to be a field value delimiter
								if (!reader.NextToken(out var expectedDelimiter) || expectedDelimiter.TokenType != RobotsPageTokenType.FieldValueDelimiter)
								{
									ReadTillNextDirective(ref reader);
									continue;
								}

								if (!reader.NextToken(out var expectedWhitespaceOrValue, RobotsPageTokenValueFormat.Flexible))
								{
									continue;
								}

								//Skip any whitespace that might exist
								if (expectedWhitespaceOrValue.TokenType == RobotsPageTokenType.Whitespace)
								{
									if (!reader.NextToken(out var expectedValue, RobotsPageTokenValueFormat.Flexible))
									{
										continue;
									}
									expectedWhitespaceOrValue = expectedValue;
								}

								if (expectedWhitespaceOrValue.TokenType == RobotsPageTokenType.Value)
								{
									var valueTokenString = expectedWhitespaceOrValue.ToString();
									var directives = GetDirectives(userAgentDirectives, userAgent);
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
							}
						default:
							{
								if (parseState != ParseState.UserAgentOrValue)
								{
									continue;
								}

								//Assume this is a user agent if we find a field value delimiter ...
								if (
									reader.NextToken(out var expectedDelimiterOrWhitespace) && 
									expectedDelimiterOrWhitespace.TokenType == RobotsPageTokenType.FieldValueDelimiter
								)
								{
									//... but only do this once in a rule
									parseState = ParseState.Value;
									userAgent = tokenValue;
								}
								continue;
							}
					}
				case RobotsPageTokenType.Whitespace:
				case RobotsPageTokenType.DirectiveDelimiter:
					continue;
				case RobotsPageTokenType.FieldValueDelimiter:
					if (parseState is not ParseState.Value)
					{
						//Unexpected token type - read till the next directive delimiter
						ReadTillNextDirective(ref reader);
					}
					continue;
			}
		}
	}
}
