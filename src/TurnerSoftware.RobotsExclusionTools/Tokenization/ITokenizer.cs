using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools.Tokenization
{
	public interface ITokenizer
	{
		IEnumerable<Token> Tokenize(string text);
		IEnumerable<Token> Tokenize(TextReader reader);
		Task<IEnumerable<Token>> TokenizeAsync(TextReader reader, CancellationToken cancellationToken = default);
	}
}
