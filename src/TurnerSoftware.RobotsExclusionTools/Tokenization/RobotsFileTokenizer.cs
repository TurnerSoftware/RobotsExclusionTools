using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public class RobotsFileTokenizer : TokenizerBase
	{
		private static readonly IEnumerable<TokenDefinition> TokenDefinitions = new List<TokenDefinition>
		{
			//Regex based on documented standard: http://www.robotstxt.org/norobots-rfc.txt

			//Token (aka field) is any character except CTLs or "tspecials" (see Page 6 of RFC listed above - also documented in RFC 1945)
			//Valid is between \x21 and \x7e (inclusive) but exluding the following:
			//\x22 = "
			//\x28 = (
			//\x29 = )
			//\x2c = ,
			//\x2f = /
			//\x3a = :
			//\x3b = ;
			//\x3c = <
			//\x3d = =
			//\x3e = >
			//\x3f = ?
			//\x40 = @
			//\x5b = [
			//\x5c = \
			//\x5d = ]
			//\x7b = {
			//\x7d = }
			//
			//This can be expressed as the following:
			//\x21\x23-\x27\x2a\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e

			new TokenDefinition(TokenType.Comment, @"^#[^\x0A\x0D]*"),
			new TokenDefinition(TokenType.Field, @"^[\x21\x23-\x27\x2a\x2b\x2d\x2e\x41-\x5a\x5e-\x7a\x7c\x7e]+(?=:[ ])"),
			new TokenDefinition(TokenType.FieldValueDelimiter, "^:[ ]"),
			new TokenDefinition(TokenType.Value, @"^[^\x0A\x0D#]+"),
			new TokenDefinition(TokenType.NewLine, @"^\x0D?\x0A")
		};

		protected override IEnumerable<TokenDefinition> GetTokenDefinitions()
		{
			return TokenDefinitions;
		}
	}
}
