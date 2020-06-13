using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization.Tokenizers
{
	public class RobotsPageTokenizer : TokenizerBase
	{
		private static readonly IEnumerable<TokenDefinition> TokenDefinitions = new []
		{
			//Based on the same character restriction rules as the RobotsFileTokenizer
			new TokenDefinition(TokenType.Field, @"\G[\x21\x23-\x27\x2a\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e]+(?=:[ ])"),
			new TokenDefinition(TokenType.FieldValueDelimiter, @"\G:[ ]"),
			new TokenDefinition(TokenType.Value, @"\G[^\x0A\x0D#,]+"),
			new TokenDefinition(TokenType.NewLine, @"\G\x0D?\x0A"),
			new TokenDefinition(TokenType.ValueDelimiter, @"\G,[ ]+")
		};
		
		protected override IEnumerable<TokenDefinition> GetTokenDefinitions()
		{
			return TokenDefinitions;
		}
	}
}
