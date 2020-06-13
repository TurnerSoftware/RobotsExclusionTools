using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public abstract class TokenPatternValidatorBase : ITokenPatternValidator
	{
		protected abstract IEnumerable<TokenPattern> LookupTokenPatterns(TokenType tokenType);

		public TokenValidationResult Validate(IEnumerable<Token> tokens)
		{
			var errors = new List<TokenValidationError>();

			var preceedingTokens = new Stack<TokenType>(tokens.Count());
			var succeedingTokens = new Queue<TokenType>(tokens.Select(t => t.TokenType).ToArray());

			var currentToken = TokenType.NotDefined;
			var lineNumber = 1;

			while (succeedingTokens.Any())
			{
				currentToken = succeedingTokens.Dequeue();

				var tokenValidation = ValidateToken(lineNumber, currentToken, preceedingTokens, succeedingTokens);
				if (tokenValidation != null)
				{
					errors.Add(tokenValidation);
				}

				preceedingTokens.Push(currentToken);

				if (currentToken == TokenType.NewLine)
				{
					lineNumber++;
				}
			}

			return new TokenValidationResult(errors);
		}
		
		private TokenValidationError ValidateToken(int lineNumber, TokenType tokenType, Stack<TokenType> preceeding, Queue<TokenType> succeeding)
		{
			var tokenPatterns = LookupTokenPatterns(tokenType);

			TokenValidationError lastError = null;
			
			foreach (var tokenPattern in tokenPatterns)
			{
				var tokenPatternError = ValidateTokenPattern(lineNumber, tokenPattern, preceeding, succeeding);
				if (tokenPatternError == null)
				{
					return null;
				}

				lastError = tokenPatternError;
			}

			return lastError;
		}

		private TokenValidationError ValidateTokenPattern(int lineNumber, TokenPattern tokenPattern, Stack<TokenType> preceeding, Queue<TokenType> succeeding)
		{
			if (tokenPattern.Preceeding.Count > 0)
			{
				if (!PatternMatch(tokenPattern.Preceeding, preceeding))
				{
					return new TokenValidationError(lineNumber, $"Preceeding tokens did not match the pattern for token {tokenPattern.Token}", tokenPattern.Preceeding, preceeding);
				}
			}

			if (tokenPattern.Succeeding.Count > 0)
			{
				if (!PatternMatch(tokenPattern.Succeeding, succeeding))
				{
					return new TokenValidationError(lineNumber, $"Succeeding tokens did not match the pattern for token {tokenPattern.Token}", tokenPattern.Succeeding, succeeding);
				}
			}

			return null;
		}

		private bool PatternMatch(IReadOnlyCollection<TokenType> initialTokens, IReadOnlyCollection<TokenType> comparisonTokens)
		{
			//Check number of tokens match (though ignore required new lines)
			//This effectively allows "NewLine" tokens to be optional when there are no proceeding tokens (eg. at the start of the file)
			if (initialTokens.Count(t => t != TokenType.NewLine) > comparisonTokens.Count)
			{
				return false;
			}

			var index = 0;
			var length = Math.Min(initialTokens.Count, comparisonTokens.Count);
			var initialTokensArray = (TokenType[])initialTokens;

			foreach (var comparisonToken in comparisonTokens)
			{
				if (index >= length)
				{
					break;
				}

				if (initialTokensArray[index] != comparisonToken)
				{
					return false;
				}

				index++;
			}

			return true;
		}
	}
}
