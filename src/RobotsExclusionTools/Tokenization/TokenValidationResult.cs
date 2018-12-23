using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotsExclusionTools.Tokenization
{
	public class TokenValidationResult
	{
		public bool IsValid { get; }
		public IEnumerable<TokenValidationError> Errors { get; }

		public TokenValidationResult(IEnumerable<TokenValidationError> errors)
		{
			Errors = errors;
			IsValid = !errors.Any();
		}
	}

	public class TokenValidationError
	{
		public int Line { get; }
		public string Message { get; }
		public IEnumerable<TokenType> Expected { get; }
		public IEnumerable<TokenType> Actual { get; }

		public TokenValidationError(int line, string message, IEnumerable<TokenType> expected, IEnumerable<TokenType> actual)
		{
			Line = line;
			Message = message;
			Expected = expected;
			Actual = actual.Take(Math.Min(actual.Count(), expected.Count())).ToArray();
		}
	}
}
