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

		private HttpClient HttpClient { get; }

		public RobotsParser() : this(new HttpClient()) { }

		public RobotsParser(HttpClient client) : this(client, new Tokenizer(), new TokenPatternValidator(), new RobotsEntryTokenParser()) { }

		public RobotsParser(HttpClient client, ITokenizer tokenizer, ITokenPatternValidator patternValidator, IRobotsEntryTokenParser tokenParser)
		{
			HttpClient = client ?? throw new ArgumentNullException(nameof(client));
			Tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
			PatternValidator = patternValidator ?? throw new ArgumentNullException(nameof(patternValidator));
			TokenParser = tokenParser ?? throw new ArgumentNullException(nameof(tokenParser));
		}
		
		public RobotsFile FromString(string robotsText, Uri baseUri)
		{
			using (var memoryStream = new MemoryStream())
			{
				var streamWriter = new StreamWriter(memoryStream);
				streamWriter.Write(robotsText);
				streamWriter.Flush();

				memoryStream.Seek(0, SeekOrigin.Begin);
				
				var streamReader = new StreamReader(memoryStream);
				var tokens = Tokenizer.Tokenize(streamReader);
				return FromTokens(tokens, baseUri);
			}
		}

		public async Task<RobotsFile> FromUriAsync(Uri robotsUri)
		{
			var baseUri = new Uri(robotsUri.GetLeftPart(UriPartial.Authority));
			robotsUri = new UriBuilder(robotsUri) { Path = "/robots.txt" }.Uri;
			
			using (var response = await HttpClient.GetAsync(robotsUri))
			{
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
					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						return await FromStreamAsync(stream, baseUri);
					}
				}
			}

			return RobotsFile.AllowAllRobots(baseUri);
		}

		public async Task<RobotsFile> FromStreamAsync(Stream stream, Uri baseUri)
		{
			var streamReader = new StreamReader(stream);
			var tokens = await Tokenizer.TokenizeAsync(streamReader);
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
