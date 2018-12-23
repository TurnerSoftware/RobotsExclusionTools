using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public interface ITokenPatternValidator
	{
		TokenValidationResult Validate(IEnumerable<Token> tokens);
	}
}
