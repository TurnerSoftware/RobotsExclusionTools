using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsEntryTokenParser : IRobotsEntryTokenParser
	{
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
			//TODO: Refactor the implementation to not be as nasty as it is :(
			var result = new List<SiteAccessEntry>();
			var parseState = new SiteAccessParseState();
			var valueSteppingTokens = new[] { TokenType.FieldValueDeliminter };
			var expectedFields = new[] { "User-agent", "Allow", "Disallow", "Crawl-delay" };

			using (var enumerator = tokens.GetEnumerator())
			{
				var lastFieldValue = string.Empty;
				while (enumerator.MoveTo(TokenType.Field))
				{
					var fieldCurrent = enumerator.Current;

					if (!expectedFields.Contains(fieldCurrent.Value))
					{
						continue;
					}

					//Reset the state when we have encountered a new "User-agent" field not immediately after another
					if (lastFieldValue != string.Empty && lastFieldValue != "User-agent" && fieldCurrent.Value == "User-agent")
					{
						result.Add(parseState.AsEntry());
						parseState.Reset();
					}
					
					//When we have seen a field for the first time that isn't a User-agent, default to all User-agents
					if (lastFieldValue == string.Empty && fieldCurrent.Value != "User-agent")
					{
						parseState.UserAgents.Add("*");
					}

					lastFieldValue = fieldCurrent.Value;

					if (fieldCurrent.Value == "User-agent")
					{
						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							parseState.UserAgents.Add(enumerator.Current.Value);
						}
					}
					else if (fieldCurrent.Value == "Allow" || fieldCurrent.Value == "Disallow")
					{
						var pathRule = fieldCurrent.Value == "Disallow" ? PathRuleType.Disallow : PathRuleType.Allow;
						var pathValue = string.Empty;

						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							pathValue = enumerator.Current.Value;
						}

						if (pathRule == PathRuleType.Allow && pathValue == null)
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
					else if (fieldCurrent.Value == "Crawl-delay")
					{
						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							if (int.TryParse(enumerator.Current.Value, out int parsedInt))
							{
								parseState.CrawlDelay = parsedInt;
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
			var valueSteppingTokens = new[] { TokenType.FieldValueDeliminter };

			using (var enumerator = tokens.GetEnumerator())
			{
				while (enumerator.MoveTo(TokenType.Field, "Sitemap"))
				{
					if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
					{
						if (Uri.TryCreate(enumerator.Current.Value, UriKind.Absolute, out Uri createdUri))
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
