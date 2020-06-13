using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public static class Extensions
	{
		public static bool MoveTo(this IEnumerator<Token> tokenEnumerator, TokenType tokenType)
		{
			return MoveTo(tokenEnumerator, t => t.TokenType == tokenType, t => false);
		}

		public static bool MoveTo(this IEnumerator<Token> tokenEnumerator, TokenType tokenType, string tokenValue)
		{
			return MoveTo(tokenEnumerator, t => t.TokenType == tokenType && t.Value == tokenValue, t => false);
		}
		
		public static bool MoveTo(this IEnumerator<Token> tokenEnumerator, IEnumerable<TokenType> anyOfTheseTypes)
		{
			return MoveTo(tokenEnumerator, t => anyOfTheseTypes.Contains(t.TokenType), t => false);
		}

		public static bool StepOverTo(this IEnumerator<Token> tokenEnumerator, TokenType tokenType, TokenType steppingToken)
		{
			return MoveTo(
				tokenEnumerator,
				t => t.TokenType == tokenType,
				t => steppingToken != t.TokenType
			);
		}

		private static bool MoveTo(IEnumerator<Token> tokenEnumerator, Func<Token, bool> stopMovingWhen, Func<Token, bool> stopEarlyWhen)
		{
			while (tokenEnumerator.MoveNext())
			{
				if (stopMovingWhen(tokenEnumerator.Current))
				{
					return true;
				}

				if (stopEarlyWhen(tokenEnumerator.Current))
				{
					return false;
				}
			}

			return false;
		}
	}
}
