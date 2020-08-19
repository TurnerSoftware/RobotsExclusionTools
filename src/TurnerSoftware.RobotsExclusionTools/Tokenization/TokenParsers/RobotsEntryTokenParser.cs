using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization.TokenParsers
{
	public class RobotsEntryTokenParser : IRobotsFileTokenParser
	{
		private const string UserAgentField = "User-agent";
		private const string DisallowField = "Disallow";
		private const string AllowField = "Allow";
		private const string CrawlDelayField = "Crawl-delay";
		private const string SitemapField = "Sitemap";

		private static readonly HashSet<string> ExpectedFields = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			UserAgentField,
			DisallowField,
			AllowField,
			CrawlDelayField
		};

		private class SiteAccessParseState
		{
			public List<string> UserAgents { get; } = new List<string>();
			public List<SiteAccessPathRule> PathRules { get; } = new List<SiteAccessPathRule>();
			public int? CrawlDelay { get; set; }

			public void Reset()
			{
				UserAgents.Clear();
				PathRules.Clear();
				CrawlDelay = null;
			}

			public SiteAccessEntry AsEntry()
			{
				return new SiteAccessEntry
				{
					UserAgents = new List<string>(UserAgents),
					PathRules = new List<SiteAccessPathRule>(PathRules),
					CrawlDelay = CrawlDelay
				};
			}
		}

		public IEnumerable<SiteAccessEntry> GetSiteAccessEntries(IEnumerable<Token> tokens)
		{
			var result = new List<SiteAccessEntry>();
			var parseState = new SiteAccessParseState();
			var comparer = StringComparer.OrdinalIgnoreCase;

			using (var enumerator = tokens.GetEnumerator())
			{
				string lastFieldValue = null;
				while (enumerator.MoveTo(TokenType.Field))
				{
					var fieldValue = enumerator.Current.Value;

					if (!ExpectedFields.Contains(fieldValue))
					{
						continue;
					}

					//Reset the state when we have encountered a new "User-agent" field not immediately after another
					if (!string.IsNullOrEmpty(lastFieldValue) && !comparer.Equals(lastFieldValue, UserAgentField) && comparer.Equals(fieldValue, UserAgentField))
					{
						result.Add(parseState.AsEntry());
						parseState.Reset();
					}
					
					//When we have seen a field for the first time that isn't a User-agent, default to all User-agents (written as "*")
					if (string.IsNullOrEmpty(lastFieldValue) && !comparer.Equals(fieldValue, UserAgentField))
					{
						parseState.UserAgents.Add("*");
					}

					lastFieldValue = fieldValue;

					if (comparer.Equals(fieldValue, UserAgentField))
					{
						if (enumerator.StepOverTo(TokenType.Value, TokenType.FieldValueDelimiter))
						{
							//NOTE: Doesn't evaluate the strict "agent" definition in Section 4 of RFC
							//		While trimming the value avoids some issues, it isn't a char-for-char accurate
							//		interpretation of the RFC and thus, is limited.
							parseState.UserAgents.Add(enumerator.Current.Value.Trim());
						}
					}
					else if (comparer.Equals(fieldValue, AllowField) || comparer.Equals(fieldValue, DisallowField))
					{
						var pathRule = comparer.Equals(fieldValue, DisallowField) ? PathRuleType.Disallow : PathRuleType.Allow;
						var pathValue = string.Empty;

						if (enumerator.StepOverTo(TokenType.Value, TokenType.FieldValueDelimiter))
						{
							//NOTE: As with the User-agent values, this doesn't evaluate the "strict" definition in the RFC
							//		Paths have specific limitations about characters that are and aren't allowed
							pathValue = enumerator.Current.Value.Trim();
						}

						if (pathRule == PathRuleType.Allow && string.IsNullOrEmpty(pathValue))
						{
							//Only disallow can be blank (no "Value" token) - See Section 4 of RFC
							continue;
						}

						parseState.PathRules.Add(new SiteAccessPathRule
						{
							RuleType = pathRule,
							Path = pathValue
						});
					}
					else if (comparer.Equals(fieldValue, CrawlDelayField))
					{
						if (enumerator.StepOverTo(TokenType.Value, TokenType.FieldValueDelimiter))
						{
							if (int.TryParse(enumerator.Current.Value, out var parsedCrawlDelay))
							{
								parseState.CrawlDelay = parsedCrawlDelay;
							}
						}
					}
				}

				result.Add(parseState.AsEntry());
			}

			return result;
		}

		public IEnumerable<SitemapUrlEntry> GetSitemapUrlEntries(IEnumerable<Token> tokens)
		{
			var result = new List<SitemapUrlEntry>();

			using (var enumerator = tokens.GetEnumerator())
			{
				while (enumerator.MoveTo(TokenType.Field, SitemapField))
				{
					if (enumerator.StepOverTo(TokenType.Value, TokenType.FieldValueDelimiter))
					{
						if (Uri.TryCreate(enumerator.Current.Value, UriKind.Absolute, out var createdUri))
						{
							result.Add(new SitemapUrlEntry
							{
								Sitemap = createdUri
							});
						}
					}
				}
			}

			return result;
		}
	}
}
