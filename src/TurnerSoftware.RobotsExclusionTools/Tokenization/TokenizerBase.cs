using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	/// <summary>
	/// Tokenizer based on Jack Vanlightly's "Simple Tokenizer" article: https://jack-vanlightly.com/blog/2016/2/3/creating-a-simple-tokenizer-lexer-in-c
	/// </summary>
	public abstract class TokenizerBase : ITokenizer
	{
		protected abstract IEnumerable<TokenDefinition> GetTokenDefinitions();

		public IEnumerable<Token> Tokenize(string text)
		{
			var tokens = new List<Token>();
			Tokenize(text, tokens);
			return tokens;
		}

		public IEnumerable<Token> Tokenize(TextReader reader)
		{
			var tokens = new List<Token>();
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				Tokenize(line, tokens);
				tokens.Add(Token.NewLineToken);
			}
			return tokens;
		}

		public async Task<IEnumerable<Token>> TokenizeAsync(TextReader reader, CancellationToken cancellationToken = default)
		{
			var tokens = new List<Token>();
			string line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				cancellationToken.ThrowIfCancellationRequested();
				Tokenize(line, tokens);
				tokens.Add(Token.NewLineToken);
			}
			return tokens;
		}

		private void Tokenize(string text, ICollection<Token> tokenCollection)
		{
			var offset = 0;
			var numberOfChars = text.Length;

			while (offset < numberOfChars)
			{
				var match = FindMatch(text, offset);
				if (match.IsMatch)
				{
					tokenCollection.Add(new Token(match.TokenType, match.Value));
					offset += match.MatchLength;
				}
				else
				{
					offset++;
				}
			}
		}

		private TokenMatch FindMatch(string text, int offset)
		{
			foreach (var tokenDefinition in GetTokenDefinitions())
			{
				var match = tokenDefinition.Match(text, offset);
				if (match.IsMatch)
				{
					return match;
				}
			}

			return new TokenMatch { IsMatch = false };
		}
	}
}
