using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class RobotsPageTokenizer : TokenizerBase
	{
		private static readonly IEnumerable<TokenDefinition> TokenDefinitions = new List<TokenDefinition>
		{
			//Based on the same character restriction rules as the RobotsFileTokenizer
			new TokenDefinition(TokenType.Field, @"^[\x21\x23-\x27\x2a\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e]+(?=:[ ])"),
			new TokenDefinition(TokenType.FieldValueDelimiter, "^:[ ]"),
			new TokenDefinition(TokenType.Value, @"^[^\x0A\x0D#]+"),
			new TokenDefinition(TokenType.NewLine, @"^\x0D?\x0A"),
			new TokenDefinition(TokenType.ValueDelimiter, "^,[ ]+")
		};
		
		protected override IEnumerable<TokenDefinition> GetTokenDefinitions()
		{
			return TokenDefinitions;
		}
	}
}
