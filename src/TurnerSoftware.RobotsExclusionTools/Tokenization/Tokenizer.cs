using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	/// <summary>
	/// Tokenizer based on Jack Vanlightly's "Simple Tokenizer" article: https://jack-vanlightly.com/blog/2016/2/3/creating-a-simple-tokenizer-lexer-in-c
	/// </summary>
	public class Tokenizer : ITokenizer
	{
		private static readonly IEnumerable<TokenDefinition> TokenDefinitions = new List<TokenDefinition>
		{
			//Regex based on documented standard: http://www.robotstxt.org/norobots-rfc.txt

			//Token (aka field) is any character except CTLs or "tspecials" (see Page 6 of RFC listed above - also documented in RFC 1945)
			//Valid is between \x21 and \x7e (inclusive) but exluding the following:
			//\x22 = "
			//\x28 = (
			//\x29 = )
			//\x2c = ,
			//\x2f = /
			//\x3a = :
			//\x3b = ;
			//\x3c = <
			//\x3d = =
			//\x3e = >
			//\x3f = ?
			//\x40 = @
			//\x5b = [
			//\x5c = \
			//\x5d = ]
			//\x7b = {
			//\x7d = }
			//
			//This can be expressed as the following:
			//\x21\x23-\x27\x2a-\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e

			new TokenDefinition(TokenType.Comment, "^#[^\x0A\x0D]*"),
			new TokenDefinition(TokenType.Field, "^[\x21\x23-\x27\x2a-\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e]+(?=:[ ])"),
			new TokenDefinition(TokenType.FieldValueDeliminter, "^:[ ]"),
			new TokenDefinition(TokenType.Value, "^[^\x0A\x0D#]+"),
			new TokenDefinition(TokenType.NewLine, "^\x0D?\x0A")
		};

		public IEnumerable<Token> Tokenize(string text)
		{
			var tokens = new List<Token>();
			var remainingText = text;

			while (!string.IsNullOrWhiteSpace(remainingText))
			{
				var match = FindMatch(remainingText);
				if (match.IsMatch)
				{
					tokens.Add(new Token(match.TokenType, match.Value));
					remainingText = match.RemainingText;
				}
				else
				{
					remainingText = remainingText.Substring(1);
				}
			}

			return tokens;
		}

		public IEnumerable<Token> Tokenize(TextReader reader)
		{
			var tokens = new List<Token>();
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				tokens.AddRange(Tokenize(line));
				tokens.Add(Token.NewLineToken);
			}
			return tokens;
		}

		public async Task<IEnumerable<Token>> TokenizeAsync(TextReader reader)
		{
			var tokens = new List<Token>();
			string line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				tokens.AddRange(Tokenize(line));
				tokens.Add(Token.NewLineToken);
			}
			return tokens;
		}

		private TokenMatch FindMatch(string text)
		{
			foreach (var tokenDefinition in TokenDefinitions)
			{
				var match = tokenDefinition.Match(text);
				if (match.IsMatch)
				{
					return match;
				}
			}

			return new TokenMatch { IsMatch = false };
		}
	}
}
