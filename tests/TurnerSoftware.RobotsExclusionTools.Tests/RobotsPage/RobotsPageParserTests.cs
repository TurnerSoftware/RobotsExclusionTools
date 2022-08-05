using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsPage
{
	[TestClass]
	public class RobotsPageParserTests : TestBase
	{
		[TestMethod]
		public void CanIndex()
		{
			var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsTrue(robotsPageDefinition.CanIndex("unlistedbot"));
			Assert.IsTrue(robotsPageDefinition.CanIndex("NoFollowBot/3000"));
			Assert.IsFalse(robotsPageDefinition.CanIndex("NoFollowNoIndexBot/2.0"));
			Assert.IsFalse(robotsPageDefinition.CanIndex("NoneBot/1.0"));
		}

		[TestMethod]
		public void CanFollowLinks()
		{
			var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsTrue(robotsPageDefinition.CanFollowLinks("unlistedbot"));
			Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoFollowBot/3000"));
			Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoFollowNoIndexBot/2.0"));
			Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoneBot/1.0"));
		}

		[TestMethod]
		public void CustomRule()
		{
			var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsFalse(robotsPageDefinition.HasRule("notranslate", "unlistedbot"));
			Assert.IsTrue(robotsPageDefinition.HasRule("notranslate", "CustomBot/5.1"));
		}

		[TestMethod]
		public void RuleWithoutAgent()
		{
			var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsTrue(robotsPageDefinition.HasRule("noarchive"));
			Assert.IsFalse(robotsPageDefinition.HasRule("notranslate"));
		}

		[TestMethod]
		public void RuleWithValueWithoutAgent()
		{
			var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsTrue(robotsPageDefinition.TryGetGlobalEntry(out var globalEntry));
			Assert.IsTrue(globalEntry.TryGetValue("unavailable_after".AsSpan(), out var value));
			Assert.AreEqual("25 Jun 2010 15:00:00 PST", value);
		}

		[TestMethod]
		public void LastRuleOverride()
		{
			var definition = GetRobotsPageDefinition("LastRuleOverride-Example.txt");

			Assert.IsTrue(definition.CanIndex("AllBot/2.5"));
			Assert.IsTrue(definition.CanFollowLinks("AllBot/2.5"));
			Assert.IsFalse(definition.CanIndex("InheritRuleBot/2.0"));
			Assert.IsTrue(definition.CanFollowLinks("InheritRuleBot/2.0"));
			Assert.IsTrue(definition.CanIndex("CustomBot/1.0"));
			Assert.IsTrue(definition.CanFollowLinks("CustomBot/1.0"));
		}
	}
}
