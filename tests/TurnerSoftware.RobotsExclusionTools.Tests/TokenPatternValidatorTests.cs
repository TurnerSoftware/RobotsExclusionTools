using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class TokenPatternValidatorTests : TestBase
	{
		[TestMethod]
		public void RFCValidPatterns()
		{
			var robots = LoadRFCExample();
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var validator = new TokenPatternValidator();
			var result = validator.Validate(tokens);

			Assert.IsTrue(result.IsValid);
		}

		[TestMethod]
		public void MalformedFieldPatterns()
		{
			var robots = LoadRobotsResource("InvalidField-Example.txt");
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var validator = new TokenPatternValidator();
			var result = validator.Validate(tokens);

			Assert.IsFalse(result.IsValid);

			var firstErrorExpectedTokens = result.Errors.First().Expected;
			Assert.AreEqual(TokenType.FieldValueDeliminter, firstErrorExpectedTokens.ElementAt(0));
			Assert.AreEqual(TokenType.Field, firstErrorExpectedTokens.ElementAt(1));
			Assert.AreEqual(2, firstErrorExpectedTokens.Count());
			Assert.AreEqual(19, result.Errors.Count());
		}

		[TestMethod]
		public void CorrectFieldPatterns()
		{
			var robots = LoadRFCExample();
			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var validator = new TokenPatternValidator();
			var result = validator.Validate(tokens);

			Assert.IsTrue(result.IsValid);
		}
	}
}
