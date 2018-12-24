using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsEntryTokenParser : IRobotsEntryTokenParser
	{
		private class SiteAccessParseState
		{
			public List<string> UserAgents { get; } = new List<string>();
			public List<string> Disallow { get; } = new List<string>();

			public List<string> Allow { get; } = new List<string>();
			public int? CrawlDelay { get; set; }

			public void Reset()
			{
				UserAgents.Clear();
				Disallow.Clear();
				Allow.Clear();
				CrawlDelay = null;
			}

			public SiteAccessEntry AsEntry()
			{
				return new SiteAccessEntry
				{
					UserAgents = new List<string>(UserAgents),
					Disallow = new List<string>(Disallow),
					Allow = new List<string>(Allow),
					CrawlDelay = CrawlDelay
				};
			}
		}

		public IEnumerable<SiteAccessEntry> GetSiteAccessEntries(IEnumerable<Token> tokens)
		{
			var result = new List<SiteAccessEntry>();
			var parseState = new SiteAccessParseState();
			var valueSteppingTokens = new[] { TokenType.FieldValueDeliminter };

			using (var enumerator = tokens.GetEnumerator())
			{
				var lastFieldValue = string.Empty;
				while (enumerator.MoveTo(TokenType.Field))
				{
					var fieldCurrent = enumerator.Current;

					//Reset the state when we have encountered a new "User-agent" field not immediately after another
					if (lastFieldValue != string.Empty && lastFieldValue != "User-agent" && fieldCurrent.Value == "User-agent")
					{
						result.Add(parseState.AsEntry());
						parseState.Reset();
					}

					lastFieldValue = fieldCurrent.Value;

					if (fieldCurrent.Value == "User-agent")
					{
						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							parseState.UserAgents.Add(enumerator.Current.Value);
						}
					}
					else if (fieldCurrent.Value == "Allow")
					{
						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							parseState.Allow.Add(enumerator.Current.Value);
						}
					}
					else if (fieldCurrent.Value == "Disallow")
					{
						if (enumerator.StepOverTo(TokenType.Value, valueSteppingTokens))
						{
							parseState.Disallow.Add(enumerator.Current.Value);
						}
						else
						{
							//Disallow can be blank (no "Value" token) - See Section 4 of RFC
							parseState.Disallow.Add(string.Empty);
						}
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
