using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotsExclusionTools.Tokenization;
using System.IO;
using System.Linq;

namespace RobotsExclusionTools.Tests
{
	[TestClass]
	public class TokenizationTests : TestBase
	{
		[TestMethod]
		public void RFCFieldTokenization()
		{
			var robots = LoadRFCExample();
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var fieldTokens = tokens.Where(t => t.TokenType == TokenType.Field);

			Assert.AreEqual(11, fieldTokens.Count());
			Assert.AreEqual(4, fieldTokens.Count(t => t.Value == "User-agent"));
			Assert.AreEqual(4, fieldTokens.Count(t => t.Value == "Disallow"));
			Assert.AreEqual(3, fieldTokens.Count(t => t.Value == "Allow"));
		}

		[TestMethod]
		public void RFCValueTokenization()
		{
			var robots = LoadRFCExample();
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var valueTokens = tokens.Where(t => t.TokenType == TokenType.Value);

			Assert.AreEqual(10, valueTokens.Count());
			Assert.AreEqual(1, valueTokens.Count(t => t.Value == "unhipbot"));
			Assert.AreEqual(2, valueTokens.Count(t => t.Value == "/"));
		}

		[TestMethod]
		public void RFCCommentTokenization()
		{
			var robots = LoadRFCExample();
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var commentTokens = tokens.Where(t => t.TokenType == TokenType.Comment);

			Assert.AreEqual(2, commentTokens.Count());
			Assert.AreEqual("# /robots.txt for http://www.fict.org/", commentTokens.First().Value);
			Assert.AreEqual("# comments to webmaster@fict.org", commentTokens.Last().Value);
		}

		[TestMethod]
		public void InvalidFields()
		{
			var robots = LoadRobotsResource("InvalidField-Example.txt");
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var fieldTokens = tokens.Where(t => t.TokenType == TokenType.Field);
			var valueTokens = tokens.Where(t => t.TokenType == TokenType.Value);

			Assert.AreEqual(0, fieldTokens.Count());
			Assert.AreEqual(19, valueTokens.Count());
		}
	}
}
