using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TurnerSoftware.RobotsExclusionTools.Tokenization;
using TurnerSoftware.RobotsExclusionTools.Tokenization.Tokenizers;
using TurnerSoftware.RobotsExclusionTools.Tokenization.Validators;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsPage
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
			Assert.AreEqual(TokenType.NotDefined, firstError.Actual.ElementAt(0));
			Assert.AreEqual(1, firstError.Actual.Count());
			Assert.AreEqual(1, result.Errors.Count());
		}
	}
}
