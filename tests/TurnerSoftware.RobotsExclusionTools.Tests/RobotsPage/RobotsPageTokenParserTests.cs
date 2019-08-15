using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TurnerSoftware.RobotsExclusionTools.Tokenization;
using TurnerSoftware.RobotsExclusionTools.Tokenization.TokenParsers;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsPage
{
	[TestClass]
	public class RobotsPageTokenParserTests : TestBase
	{
		[TestMethod]
		public void SplitRuleNoneToNoIndexAndNoFollow()
		{
			var parser = new RobotsPageTokenParser();
			var tokens = new[]
			{
				new Token(TokenType.Value, "none"),
				Token.NewLineToken,
				new Token(TokenType.Field, "otherbot"),
				new Token(TokenType.FieldValueDelimiter, null),
				new Token(TokenType.Value, "none")
			};

			var entries = parser.GetPageAccessEntries(tokens);

			Assert.AreEqual(2, entries.Count());

			var globalRule = entries.First(e => e.UserAgent == "*");
			Assert.IsFalse(globalRule.Rules.Any(r => r.RuleName == "none"));
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "noindex"));
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "nofollow"));

			var agentRule = entries.First(e => e.UserAgent == "otherbot");
			Assert.IsFalse(agentRule.Rules.Any(r => r.RuleName == "none"));
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "noindex"));
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "nofollow"));
		}

		[TestMethod]
		public void UnavailableAfterRuleIsParsed()
		{
			var parser = new RobotsPageTokenParser();
			var tokens = new[]
			{
				new Token(TokenType.Field, "unavailable_after"),
				new Token(TokenType.FieldValueDelimiter, null),
				new Token(TokenType.Value, "25 Jun 2010 15:00:00 PST")
			};

			var entries = parser.GetPageAccessEntries(tokens);

			Assert.AreEqual(1, entries.Count());

			var globalRule = entries.First(e => e.UserAgent == "*");
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "unavailable_after" && r.RuleValue == "25 Jun 2010 15:00:00 PST"));
		}

		[TestMethod]
		public void MultiValueParsing()
		{
			var parser = new RobotsPageTokenParser();
			var tokens = new[]
			{
				new Token(TokenType.Value, "outerValue1"),
				new Token(TokenType.ValueDelimiter, null),
				new Token(TokenType.Value, "outerValue2"),
				Token.NewLineToken,
				new Token(TokenType.Field, "otherbot"),
				new Token(TokenType.FieldValueDelimiter, null),
				new Token(TokenType.Value, "innerValue1"),
				new Token(TokenType.ValueDelimiter, null),
				new Token(TokenType.Value, "innerValue2")
			};

			var entries = parser.GetPageAccessEntries(tokens);

			Assert.AreEqual(2, entries.Count());

			var globalRule = entries.First(e => e.UserAgent == "*");
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "outerValue1"));
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "outerValue2"));

			var agentRule = entries.First(e => e.UserAgent == "otherbot");
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "innerValue1"));
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "innerValue2"));
		}

		[TestMethod]
		public void GlobalRulesInAgentRulesList()
		{
			var parser = new RobotsPageTokenParser();
			var tokens = new[]
			{
				new Token(TokenType.Value, "outerValue"),
				Token.NewLineToken,
				new Token(TokenType.Field, "otherbot"),
				new Token(TokenType.FieldValueDelimiter, null),
				new Token(TokenType.Value, "innerValue")
			};

			var entries = parser.GetPageAccessEntries(tokens);

			Assert.AreEqual(2, entries.Count());

			var globalRule = entries.First(e => e.UserAgent == "*");
			Assert.IsTrue(globalRule.Rules.Any(r => r.RuleName == "outerValue"));
			Assert.IsFalse(globalRule.Rules.Any(r => r.RuleName == "innerValue"));

			var agentRule = entries.First(e => e.UserAgent == "otherbot");
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "outerValue"));
			Assert.IsTrue(agentRule.Rules.Any(r => r.RuleName == "innerValue"));

			var ruleIndexes = agentRule.Rules.Select((entry, indexPosition) => new
			{
				Index = indexPosition,
				Entry = entry
			});
			_ = ruleIndexes.First(r => r.Entry.RuleName == "outerValue");
			_ = ruleIndexes.First(r => r.Entry.RuleName == "innerValue");
		}


		[TestMethod]
		public void GlobalRulesBeforeAgentSpecificRules()
		{
			var parser = new RobotsPageTokenParser();
			var tokens = new[]
			{
				new Token(TokenType.Value, "outerValue"),
				Token.NewLineToken,
				new Token(TokenType.Field, "otherbot"),
				new Token(TokenType.FieldValueDelimiter, null),
				new Token(TokenType.Value, "innerValue")
			};

			var entries = parser.GetPageAccessEntries(tokens);
			var agentRule = entries.First(e => e.UserAgent == "otherbot");

			var ruleIndexes = agentRule.Rules.Select((entry, indexPosition) => new
			{
				Index = indexPosition,
				Entry = entry
			});
			var outerRule = ruleIndexes.First(r => r.Entry.RuleName == "outerValue");
			var innerRule = ruleIndexes.First(r => r.Entry.RuleName == "innerValue");

			Assert.AreEqual(0, outerRule.Index);
			Assert.AreEqual(1, innerRule.Index);
		}
	}
}
