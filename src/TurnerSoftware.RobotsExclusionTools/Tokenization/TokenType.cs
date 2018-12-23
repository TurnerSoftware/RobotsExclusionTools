using System;
using System.Collections.Generic;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public enum TokenType
	{
		NotDefined,
		Field,
		Value,
		Comment,
		FieldValueDeliminter,
		NewLine
	}
}
