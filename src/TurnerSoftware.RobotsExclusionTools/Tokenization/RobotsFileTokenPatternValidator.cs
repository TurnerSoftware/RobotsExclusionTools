using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class RobotsFileTokenPatternValidator : TokenPatternValidatorBase
	{
		private static readonly Dictionary<TokenType, IEnumerable<TokenPattern>> TokenPatterns = new Dictionary<TokenType, IEnumerable<TokenPattern>>
		{
			{
				TokenType.Comment,
				new[]
				{
					new TokenPattern(TokenType.Comment, null, new [] { TokenType.NewLine })
				}
			},
			{
				TokenType.Field,
				new[]
				{
					new TokenPattern(TokenType.Field, new[]{ TokenType.NewLine }, new [] { TokenType.FieldValueDelimiter })
				}
			},
			{
				TokenType.Value,
				new[]
				{
					new TokenPattern(TokenType.Value, new[]{ TokenType.FieldValueDelimiter, TokenType.Field }, null)
				}
			},
			{
				TokenType.FieldValueDelimiter,
				new[]
				{
					new TokenPattern(TokenType.FieldValueDelimiter, new[] { TokenType.Field }, null)
				}
			}
		};

		protected override IEnumerable<TokenPattern> LookupTokenPatterns(TokenType tokenType)
		{
			if (TokenPatterns.ContainsKey(tokenType))
			{
				return TokenPatterns[tokenType];
			}
			
			return Enumerable.Empty<TokenPattern>();
		}
	}
}
