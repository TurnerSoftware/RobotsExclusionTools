using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurnerSoftware.RobotsExclusionTools.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class RobotsPageTokenPatternValidatorTests : TestBase
	{
		[TestMethod]
		public void ValidPatterns()
		{
			var robots = LoadResource("RobotsPage/RobotsPage-Example.txt");
			var tokenizer = new RobotsPageTokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var validator = new RobotsPageTokenPatternValidator();
			var result = validator.Validate(tokens);

			Assert.IsTrue(result.IsValid);
		}

		[TestMethod]
		public void MalformedFieldPatterns()
		{
			var robots = LoadResource("RobotsPage/InvalidField-Example.txt");
			var tokenizer = new RobotsPageTokenizer();
			var tokens = tokenizer.Tokenize(robots);

			var validator = new RobotsPageTokenPatternValidator();
			var result = validator.Validate(tokens);

			Assert.IsFalse(result.IsValid);

			var firstError = result.Errors.First();
			Assert.AreEqual(TokenType.NewLine, firstError.Expected.ElementAt(0));
			Assert.AreEqual(1, firstError.Expected.Count());
			Assert.AreEqual(TokenType.Value, firstError.Actual.ElementAt(0));
			Assert.AreEqual(1, firstError.Actual.Count());
			Assert.AreEqual(1, result.Errors.Count());
		}
	}
}
