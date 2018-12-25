using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace TurnerSoftware.RobotsExclusionTools
{
	public class RobotsParser : IRobotsParser
	{
		private ITokenizer Tokenizer { get; }
		private ITokenPatternValidator PatternValidator { get; }
		private IRobotsEntryTokenParser TokenParser { get; }

		public RobotsParser() : this(new Tokenizer(), new TokenPatternValidator(), new RobotsEntryTokenParser()) { }

		public RobotsParser(ITokenizer tokenizer, ITokenPatternValidator patternValidator, IRobotsEntryTokenParser tokenParser)
		{
			Tokenizer = tokenizer;
			PatternValidator = patternValidator;
			TokenParser = tokenParser;
		}

		
		public RobotsFile FromString(string robotsText, Uri baseUri)
		{
			var tokens = Tokenizer.Tokenize(robotsText);
			return FromTokens(tokens, baseUri);
		}

		public async Task<RobotsFile> FromUriAsync(Uri robotsUri)
		{
			var baseUri = new Uri(robotsUri.GetLeftPart(UriPartial.Authority));
			robotsUri = new UriBuilder(robotsUri) { Path = "/robots.txt" }.Uri;

			using (var client = new HttpClient())
			{
				var response = await client.GetAsync(robotsUri);
				if (response.StatusCode == HttpStatusCode.NotFound)
				{
					return RobotsFile.AllowAllRobots(baseUri);
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
				{
					return RobotsFile.DenyAllRobots(baseUri);
				}
				else if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
				{
					var robotsText = await response.Content.ReadAsStringAsync();
					return FromString(robotsText, baseUri);
				}
			}

			return RobotsFile.AllowAllRobots(baseUri);
		}

		public async Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri)
		{
			var tokens = new List<Token>();

			using (var streamReader = new StreamReader(stream))
			{
				while (!streamReader.EndOfStream)
				{
					var robotsLine = await streamReader.ReadLineAsync();
					tokens.AddRange(Tokenizer.Tokenize(robotsLine));
					tokens.Add(new Token(TokenType.NewLine, "\n"));
				}
			}

			return FromTokens(tokens, baseUri);
		}

		private RobotsFile FromTokens(IEnumerable<Token> tokens, Uri baseUri)
		{
			var validationResult = PatternValidator.Validate(tokens);

			if (validationResult.IsValid)
			{
				return new RobotsFile(baseUri)
				{
					SiteAccessEntries = TokenParser.GetSiteAccessEntries(tokens),
					SitemapEntries = TokenParser.GetSitemapUrlEntries(tokens)
				};
			}

			//TODO: Probably throw an exception of some sort (or maybe make it optional to continue on validation failure)
			return null;
		}
	}
}
