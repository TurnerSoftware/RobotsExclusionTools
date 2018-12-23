using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotsExclusionTools.Tokenization
{
	public class TokenPatternValidator
	{
		private static readonly Dictionary<TokenType, TokenPattern> TokenPattern = new Dictionary<TokenType, TokenPattern>
		{
			{ TokenType.Comment, new TokenPattern(TokenType.Comment, null, new [] { TokenType.NewLine }) },
			{ TokenType.Field, new TokenPattern(TokenType.Field, new[]{ TokenType.NewLine }, new [] { TokenType.FieldValueDeliminter }) },
			{ TokenType.Value, new TokenPattern(TokenType.Value, new[]{ TokenType.FieldValueDeliminter, TokenType.Field }, null) },
			{ TokenType.FieldValueDeliminter, new TokenPattern(TokenType.FieldValueDeliminter, new[] { TokenType.Field }, null) }
		};

		public TokenValidationResult Validate(IEnumerable<Token> tokens)
		{
			var errors = new List<TokenValidationError>();

			var preceedingTokens = new Stack<TokenType>();
			var succeedingTokens = new Queue<TokenType>(tokens.Select(t => t.TokenType));

			var currentToken = TokenType.NotDefined;

			while (succeedingTokens.Any())
			{
				currentToken = succeedingTokens.Dequeue();
				var tokenValidation = ValidateToken(currentToken, preceedingTokens, succeedingTokens);
				if (tokenValidation != null)
				{
					errors.Add(tokenValidation);
				}

				preceedingTokens.Push(currentToken);
			}

			return new TokenValidationResult(errors);
		}

		private TokenValidationError ValidateToken(TokenType tokenType, Stack<TokenType> preceeding, Queue<TokenType> succeeding)
		{
			if (!TokenPattern.ContainsKey(tokenType))
			{
				return null;
			}

			var lineNumber = preceeding.Count(t => t == TokenType.NewLine) + 1;

			var pattern = TokenPattern[tokenType];
			if (pattern.Preceeding.Any())
			{
				if (!PatternMatch(pattern.Preceeding, preceeding))
				{
					return new TokenValidationError(lineNumber, $"Proceeding tokens did not match the pattern for token {tokenType}", pattern.Preceeding, preceeding);
				}
			}
			
			if (pattern.Succeeding.Any())
			{
				if (!PatternMatch(pattern.Succeeding, succeeding))
				{
					return new TokenValidationError(lineNumber, $"Succeeding tokens did not match the pattern for token {tokenType}", pattern.Succeeding, succeeding);
				}
			}

			return null;
		}

		private bool PatternMatch(IEnumerable<TokenType> initialTokens, IEnumerable<TokenType> comparisonTokens)
		{
			//Check number of tokens match (though ignore required new lines)
			//This effectively allows "NewLine" tokens to be optional when there are no proceeding tokens (eg. at the start of the file)
			if (initialTokens.Count(t => t != TokenType.NewLine) > comparisonTokens.Count())
			{
				return false;
			}

			var sliceSize = Math.Min(initialTokens.Count(), comparisonTokens.Count());
			var slicedTokensArray = comparisonTokens.Take(sliceSize).ToArray();
			var initialTokensArray = initialTokens.ToArray();

			for (int i = 0, l = sliceSize; i < l; i++)
			{
				if (initialTokensArray[i] != slicedTokensArray[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}
