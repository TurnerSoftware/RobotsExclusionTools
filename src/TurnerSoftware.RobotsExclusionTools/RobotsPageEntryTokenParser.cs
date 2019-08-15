using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

namespace TurnerSoftware.RobotsExclusionTools
{
	/// <summary>
	/// Based on the rules defined by Google's documentation for Robots Meta Tag
	/// https://developers.google.com/search/reference/robots_meta_tag
	/// </summary>
	public class RobotsPageEntryTokenParser : IRobotsPageEntryTokenParser
	{
		private class PageAccessParseState
		{
			public string UserAgent { get; set; }
			public string Field { get; set; }
			public List<string> Values { get; } = new List<string>();
		}
		
		public IEnumerable<PageAccessEntry> GetPageAccessEntries(IEnumerable<Token> tokens)
		{
			var processedStates = new List<PageAccessParseState>();
			var parseState = new PageAccessParseState();
			var moveTokens = new[] { TokenType.Field, TokenType.Value, TokenType.NewLine };
			
			using (var enumerator = tokens.GetEnumerator())
			{
				while (enumerator.MoveTo(moveTokens))
				{
					var current = enumerator.Current;
					
					if (current.TokenType == TokenType.NewLine)
					{
						processedStates.Add(parseState);
						parseState = new PageAccessParseState();
					}
					else if (current.TokenType == TokenType.Field)
					{
						if (current.Value.IndexOf("unavailable_after", StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							parseState.Field = current.Value;
						}
						else
						{
							parseState.UserAgent = current.Value;
						}
					}
					else
					{
						parseState.Values.Add(current.Value);
					}
				}
			}
			
			PageAccessRule[] ConvertToRules(IEnumerable<PageAccessParseState> userAgentStates)
			{
				return userAgentStates.SelectMany(s => s.Values.Select(v => new PageAccessRule
				{
					//Everything is a field (noindex, nofollow etc)
					RuleName = s.Field ?? v,
					//Only "unavailable_after" has a value
					RuleValue = s.Field == null ? null : v
				}))
				//Squish multiple of the same-name rules together
				.GroupBy(r => r.RuleName, StringComparer.InvariantCultureIgnoreCase)
				.Select(rg => rg.LastOrDefault())
				.ToArray();
			}

			var globalRules = processedStates.Where(s => s.UserAgent == null).ToArray();

			var result = processedStates
				//Merge variations of User Agent definitions (case insensitive)
				.GroupBy(s => s.UserAgent, StringComparer.InvariantCultureIgnoreCase)
				.Select(g =>
					new PageAccessEntry
					{
						UserAgent = g.Key ?? "*",
						Rules = ConvertToRules(g.Key == null ? g : globalRules.Concat(g))
					}
				).ToArray();

			return result;
		}
	}
}
