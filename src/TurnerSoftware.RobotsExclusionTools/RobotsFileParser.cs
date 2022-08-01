using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System.Buffers;
using TurnerSoftware.RobotsExclusionTools.Helpers;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools;

public class RobotsFileParser : IRobotsFileParser
{
	private readonly HttpClient HttpClient;

	public RobotsFileParser() : this(new HttpClient()) { }

	public RobotsFileParser(HttpClient httpClient)
	{
		HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	/// <inheritdoc/>
	public async Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri, CancellationToken cancellationToken = default)
	{
		var parseState = new SiteAccessParseState();

		await foreach (var charLine in StreamLineReader.EnumerateLinesOfCharsAsync(stream, cancellationToken))
		{
			var reader = new RobotsFileTokenReader(charLine);
			if (reader.NextToken(out var token, RobotsFileTokenValueFormat.RuleName))
			{
				ProcessLine(token, ref reader, ref parseState);
			}
		}

		//Capture final entry that was being constructed in our state tracker
		parseState.CaptureEntry();
		return new RobotsFile(baseUri, parseState.SiteAccessEntries, parseState.SitemapUrlEntries);
	}

	/// <inheritdoc/>
	public RobotsFile FromString(string robotsText, Uri baseUri)
	{
		var reader = new RobotsFileTokenReader(robotsText.AsMemory());
		var parseState = new SiteAccessParseState();

		while (reader.NextToken(out var token, RobotsFileTokenValueFormat.RuleName))
		{
			ProcessLine(token, ref reader, ref parseState);
		}

		//Add final entry that was being constructed in our state tracker
		parseState.CaptureEntry();
		return new RobotsFile(baseUri, parseState.SiteAccessEntries, parseState.SitemapUrlEntries);
	}

	/// <inheritdoc/>
	public Task<RobotsFile> FromUriAsync(Uri robotsUri, CancellationToken cancellationToken = default)
	{
		return FromUriAsync(robotsUri, RobotsFileAccessRules.NoRobotsRfc, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<RobotsFile> FromUriAsync(Uri robotsUri, RobotsFileAccessRules accessRules, CancellationToken cancellationToken = default)
	{
		var baseUri = new Uri(robotsUri.GetLeftPart(UriPartial.Authority));
		robotsUri = new UriBuilder(robotsUri) { Path = "/robots.txt" }.Uri;

		using (var response = await HttpClient.GetAsync(robotsUri, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested(); // '.NET Framework' and '.NET Core 2.1' workaround

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return RobotsFile.ConditionalRobots(baseUri, accessRules.AllowAllWhen404NotFound);
			}
			else if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				return RobotsFile.ConditionalRobots(baseUri, accessRules.AllowAllWhen401Unauthorized);
			}
			else if (response.StatusCode == HttpStatusCode.Forbidden)
			{
				return RobotsFile.ConditionalRobots(baseUri, accessRules.AllowAllWhen403Forbidden);
			}
			else if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
			{
				using var stream = await response.Content.ReadAsStreamAsync();
				cancellationToken.ThrowIfCancellationRequested();
				return await FromStreamAsync(stream, baseUri, cancellationToken);
			}
		}

		return RobotsFile.AllowAllRobots(baseUri);
	}

	private struct SiteAccessParseState
	{
		public List<string> UserAgents { get; }
		public List<SiteAccessPathRule> PathRules { get; }
		public int? CrawlDelay { get; set; }
		public bool HasSeenRule { get; set; }

		private readonly List<SiteAccessEntry> siteAccessEntries;
		public IReadOnlyList<SiteAccessEntry> SiteAccessEntries => siteAccessEntries;

		public List<SitemapUrlEntry> SitemapUrlEntries { get; }

		public SiteAccessParseState()
		{
			UserAgents = new();
			PathRules = new();
			CrawlDelay = null;
			HasSeenRule = false;

			siteAccessEntries = new();
			SitemapUrlEntries = new();
		}

		/// <summary>
		/// Captures the currently tracked entry and resets the state to start tracking another.
		/// </summary>
		/// <remarks>
		/// A tracked entry is only captured if there are tracked user-agents and any rule has been seen.
		/// </remarks>
		public void CaptureEntry()
		{
			if (UserAgents.Count > 0 && HasSeenRule)
			{
				siteAccessEntries.Add(new SiteAccessEntry
				{
					UserAgents = UserAgents.ToArray(),
					PathRules = PathRules.Count > 0 ? PathRules.ToArray() : Array.Empty<SiteAccessPathRule>(),
					CrawlDelay = CrawlDelay
				});
			}

			UserAgents.Clear();
			PathRules.Clear();
			CrawlDelay = null;
			HasSeenRule = false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AreEqual(ReadOnlySpan<char> input, string comparison) => input.Equals(comparison.AsSpan(), StringComparison.OrdinalIgnoreCase);

	private static bool TrySkipFieldToValue(ref RobotsFileTokenReader reader, out RobotsFileToken token)
	{
		if (!reader.NextToken(out var expectedDelimiter) || expectedDelimiter.TokenType != RobotsFileTokenType.Delimiter)
		{
			goto UnexpectedToken;
		}

		if (!reader.NextToken(out var expectedWhitespaceOrValue))
		{
			goto UnexpectedToken;
		}

		if (expectedWhitespaceOrValue.TokenType == RobotsFileTokenType.Whitespace)
		{
			if (!reader.NextToken(out var expectedValue))
			{
				goto UnexpectedToken;
			}
			expectedWhitespaceOrValue = expectedValue;
		}

		if (expectedWhitespaceOrValue.TokenType == RobotsFileTokenType.Value)
		{
			token = expectedWhitespaceOrValue;
			return true;
		}

		UnexpectedToken:
		token = default;
		return false;
	}

	private static void ProcessLine(RobotsFileToken token, ref RobotsFileTokenReader reader, ref SiteAccessParseState parseState)
	{
		if (token.TokenType is RobotsFileTokenType.Whitespace && !reader.NextToken(out token, RobotsFileTokenValueFormat.RuleName))
		{
			return;
		}
		else if (token.TokenType is RobotsFileTokenType.Value)
		{
			var tokenValue = token.Value.Span;
			if (AreEqual(tokenValue, Constants.UserAgentField))
			{
				if (parseState.HasSeenRule)
				{
					parseState.CaptureEntry();
				}

				if (TrySkipFieldToValue(ref reader, out token) && token.IsValidIdentifier())
				{
					parseState.UserAgents.Add(token.ToString());
				}

				reader.SkipLine();
			}
			else
			{
				if (AreEqual(tokenValue, Constants.DisallowField) || AreEqual(tokenValue, Constants.AllowField))
				{
					var ruleType = PathRuleType.Disallow;
					if (tokenValue[0] is 'A' or 'a')
					{
						ruleType = PathRuleType.Allow;
					}

					//When we have seen a field for the first time that isn't a "User-agent", default to any user agent (written as "*")
					if (parseState.UserAgents.Count == 0)
					{
						parseState.UserAgents.Add(Constants.UserAgentWildcard);
					}

					//Ensure our next token is a delimiter, otherwise skip the entire line
					if (!reader.NextToken(out var expectedDelimiter) || expectedDelimiter.TokenType != RobotsFileTokenType.Delimiter)
					{
						reader.SkipLine();
						return;
					}

					//As long as we have a delimiter, we can accept this as a blank value if there are no more tokens
					if (!reader.NextToken(out token))
					{
						goto AcceptBlankValue;
					}

					//Step over whitespace (though if we are at the end, accept it as a blank value)
					if (token.TokenType == RobotsFileTokenType.Whitespace && !reader.NextToken(out token))
					{
						goto AcceptBlankValue;
					}

					switch (token.TokenType)
					{
						//We can have an empty value - we detect this by a comment or a new line token
						case RobotsFileTokenType.NewLine:
						case RobotsFileTokenType.Comment:
							goto AcceptBlankValue;
						//If we have a value, check it as a path
						case RobotsFileTokenType.Value when token.IsValidPath():
							parseState.HasSeenRule = true;
							parseState.PathRules.Add(new SiteAccessPathRule
							{
								RuleType = ruleType,
								Path = token.ToString()
							});
							reader.SkipLine();
							return;
						//Anything else, we ignore the entire declaration
						default:
							reader.SkipLine();
							return;
					}

					AcceptBlankValue:
					parseState.HasSeenRule = true;
					parseState.PathRules.Add(new SiteAccessPathRule(string.Empty, ruleType));
					reader.SkipLine();
				}
				else if (AreEqual(tokenValue, Constants.CrawlDelayField))
				{
					if (TrySkipFieldToValue(ref reader, out token))
					{
						var localTokenValue = token.Value.Span;
						if (PerfUtilities.TryParseInteger(localTokenValue, out var parsedCrawlDelay))
						{
							parseState.HasSeenRule = true;
							parseState.CrawlDelay = parsedCrawlDelay;
						}
					}
					reader.SkipLine();
				}
				else if (AreEqual(tokenValue, Constants.SitemapField))
				{
					if (TrySkipFieldToValue(ref reader, out token))
					{
						var tokenString = token.Value.Span.ToString();
						if (Uri.TryCreate(tokenString, UriKind.Absolute, out var parsedUri))
						{
							parseState.HasSeenRule = true;
							parseState.SitemapUrlEntries.Add(new SitemapUrlEntry(parsedUri));
						}
					}
					reader.SkipLine();
				}
			}
		}
		else if (token.TokenType is not RobotsFileTokenType.NewLine)
		{
			//If it doesn't start with a value, skip the line
			reader.SkipLine();
		}
	}
}
