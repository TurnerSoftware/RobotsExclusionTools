using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Runtime.CompilerServices;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

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
	public RobotsFile FromString(string robotsText, Uri baseUri)
	{
		Parse(robotsText.AsMemory(), out var siteAccessEntries, out var sitemapEntries);
		return new RobotsFile(baseUri, siteAccessEntries, sitemapEntries);
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

	/// <inheritdoc/>
	public async Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri, CancellationToken cancellationToken = default)
	{
		var streamReader = new StreamReader(stream);
		var robotsText = await streamReader.ReadToEndAsync();
		return FromString(robotsText, baseUri);
	}

	private struct SiteAccessParseState
	{
		public List<string> UserAgents { get; private set; }
		public List<SiteAccessPathRule> PathRules { get; private set; }
		public int? CrawlDelay { get; set; }

		public static SiteAccessParseState Create() => new()
		{
			UserAgents = new List<string>(),
			PathRules = new List<SiteAccessPathRule>()
		};

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
				UserAgents = UserAgents.ToArray(),
				PathRules = PathRules.ToArray(),
				CrawlDelay = CrawlDelay
			};
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AreEqual(ReadOnlySpan<char> input, string comparison) => input.Equals(comparison.AsSpan(), StringComparison.OrdinalIgnoreCase);

	private static bool SkipFieldToValue(ref RobotsFileTokenReader reader, RobotsFileTokenValueFormat valueFormat, out RobotsFileToken token)
	{
		if (!reader.NextToken(out var expectedDelimiter) || expectedDelimiter.TokenType != RobotsFileTokenType.Delimiter)
		{
			goto UnexpectedToken;
		}

		if (!reader.NextToken(out var expectedWhitespaceOrValue, valueFormat))
		{
			goto UnexpectedToken;
		}

		if (expectedWhitespaceOrValue.TokenType == RobotsFileTokenType.Whitespace)
		{
			if (!reader.NextToken(out var expectedValue, valueFormat))
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

	private void Parse(ReadOnlyMemory<char> value, out IReadOnlyCollection<SiteAccessEntry> siteAccessEntries, out IReadOnlyCollection<SitemapUrlEntry> sitemapEntries)
	{
		var reader = new RobotsFileTokenReader(value);
		var parseState = SiteAccessParseState.Create();
		
		var tmpSiteAccessEntries = new List<SiteAccessEntry>();
		var tmpSitemapEntries = new List<SitemapUrlEntry>();

		RobotsFileToken lastToken = default;

		while (reader.NextToken(out var token, RobotsFileTokenValueFormat.Token))
		{
			switch (token.TokenType)
			{
				case RobotsFileTokenType.Value:
					var tokenValue = token.Value.Span;
					if (AreEqual(tokenValue, Constants.DisallowField))
					{
						//When we have seen a field for the first time that isn't a "User-agent", default to any user agent (written as "*")
						if (lastToken.Value.IsEmpty && parseState.UserAgents.Count == 0)
						{
							parseState.UserAgents.Add(Constants.UserAgentWildcard);
						}

						//Ensure our next token is a delimiter, otherwise skip the entire line
						if (!reader.NextToken(out var expectedDelimiter) || expectedDelimiter.TokenType != RobotsFileTokenType.Delimiter)
						{
							reader.SkipLine();
							continue;
						}

						//As long as we have a delimiter, we can accept this as a blank disallow if there are no more tokens
						if (!reader.NextToken(out token, RobotsFileTokenValueFormat.Path))
						{
							goto AcceptBlankDisallow;
						}

						//Step over whitespace (though if we are at the end, accept it as a blank disallow)
						if (token.TokenType == RobotsFileTokenType.Whitespace && !reader.NextToken(out token, RobotsFileTokenValueFormat.Path))
						{
							goto AcceptBlankDisallow;
						}

						switch (token.TokenType)
						{
							//Disallow lines can have an empty value - we detect this by a comment or a new line token
							case RobotsFileTokenType.NewLine:
							case RobotsFileTokenType.Comment:
								goto AcceptBlankDisallow;
							//If we have a value, parse it as a path
							case RobotsFileTokenType.Value:
								parseState.PathRules.Add(new SiteAccessPathRule
								{
									RuleType = PathRuleType.Disallow,
									Path = token.ToString()
								});
								goto AcceptToken;
							//Anything else, we ignore the entire declaration
							default:
								reader.SkipLine();
								continue;
						}

						AcceptBlankDisallow:
						parseState.PathRules.Add(new SiteAccessPathRule(string.Empty, PathRuleType.Disallow));
						goto AcceptToken;
					}
					else if (AreEqual(tokenValue, Constants.UserAgentField))
					{
						//Reset the state when we have encountered a new "User-agent" field not immediately after another
						if (!lastToken.Value.IsEmpty && !AreEqual(lastToken.Value.Span, Constants.UserAgentField))
						{
							tmpSiteAccessEntries.Add(parseState.AsEntry());
							parseState.Reset();
						}

						//The value format "token" is the same syntax as "agent"
						if (SkipFieldToValue(ref reader, RobotsFileTokenValueFormat.Token, out token))
						{
							parseState.UserAgents.Add(token.ToString());
							goto AcceptToken;
						}

						reader.SkipLine();
						continue;
					}
					else if (AreEqual(tokenValue, Constants.AllowField))
					{
						//When we have seen a field for the first time that isn't a "User-agent", default to any user agent (written as "*")
						if (lastToken.Value.IsEmpty && parseState.UserAgents.Count == 0)
						{
							parseState.UserAgents.Add(Constants.UserAgentWildcard);
						}

						//An allow line requires an "rpath" value
						if (SkipFieldToValue(ref reader, RobotsFileTokenValueFormat.RPath, out token))
						{
							parseState.PathRules.Add(new SiteAccessPathRule(token.ToString(), PathRuleType.Allow));
							goto AcceptToken;
						}

						reader.SkipLine();
						continue;
					}
					else if (AreEqual(tokenValue, Constants.CrawlDelayField))
					{
						if (SkipFieldToValue(ref reader, RobotsFileTokenValueFormat.Value, out token))
						{
							var localTokenValue = token.Value.Span;
#if NETSTANDARD2_0
							if (int.TryParse(localTokenValue.ToString(), out var parsedCrawlDelay))
#else
							//We can have allocation-free integer parsing in .NET Standard 2.1
							if (int.TryParse(localTokenValue, out var parsedCrawlDelay))
#endif
							{
								parseState.CrawlDelay = parsedCrawlDelay;
							}
						}
					}
					else if (AreEqual(tokenValue, Constants.SitemapField))
					{
						if (SkipFieldToValue(ref reader, RobotsFileTokenValueFormat.Value, out token))
						{
							var tokenString = token.Value.Span.ToString();
							if (Uri.TryCreate(tokenString, UriKind.Absolute, out var parsedUri))
							{
								tmpSitemapEntries.Add(new SitemapUrlEntry(parsedUri));
								goto AcceptToken;
							}
						}
						reader.SkipLine();
						continue;
					}
					break;
				case RobotsFileTokenType.NewLine:
					goto AcceptToken;
				default:
					//If it doesn't start with a value, skip the line
					reader.SkipLine();
					continue;
			}
			AcceptToken:
			lastToken = token;
		}

		//Add final entry that was being constructed in our state tracker
		tmpSiteAccessEntries.Add(parseState.AsEntry());

		siteAccessEntries = tmpSiteAccessEntries;
		sitemapEntries = tmpSitemapEntries;
	}
}
