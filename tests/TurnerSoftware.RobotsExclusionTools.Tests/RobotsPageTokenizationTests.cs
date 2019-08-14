using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tokenization;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class RobotsPageTokenizationTests : TestBase
	{		
		[TestMethod]
		public void FieldTokenization()
		{
			var tokenizer = new RobotsPageTokenizer();
			var tokens = tokenizer.Tokenize(LoadResource("RobotsPage/RobotsPage-Example.txt"));

			var fieldTokens = tokens.Where(t => t.TokenType == TokenType.Field);

			Assert.AreEqual(5, fieldTokens.Count());
			Assert.AreEqual(2, fieldTokens.Count(t => t.Value == "googlebot"));
			Assert.AreEqual(1, fieldTokens.Count(t => t.Value == "otherbot"));
			Assert.AreEqual(1, fieldTokens.Count(t => t.Value == "somebot"));
			Assert.AreEqual(1, fieldTokens.Count(t => t.Value == "unavailable_after"));
		}

		[TestMethod]
		public void ValueTokenization()
		{
			var tokenizer = new RobotsPageTokenizer();
			var tokens = tokenizer.Tokenize(LoadResource("RobotsPage/RobotsPage-Example.txt"));

			var valueTokens = tokens.Where(t => t.TokenType == TokenType.Value);

			Assert.AreEqual(7, valueTokens.Count());
			Assert.AreEqual(2, valueTokens.Count(t => t.Value == "nofollow"));
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "noindex"));
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "none"));
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "notranslate"));
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "noarchive"));
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "25 Jun 2010 15:00:00 PST"));
		}
	}
}
