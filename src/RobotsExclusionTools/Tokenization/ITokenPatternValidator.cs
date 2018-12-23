using System;
using System.Collections.Generic;
using System.Text;

namespace RobotsExclusionTools.Tokenization
{
	public interface ITokenPatternValidator
	{
		TokenValidationResult Validate(IEnumerable<Token> tokens);
	}
}
