using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests.RobotsPage;

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
		Assert.IsFalse(robotsPageDefinition.CanIndex("NoSpaces/1.0"));
	}

	[TestMethod]
	public void CanFollowLinks()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.CanFollowLinks("unlistedbot"));
		Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoFollowBot/3000"));
		Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoFollowNoIndexBot/2.0"));
		Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoneBot/1.0"));
		Assert.IsFalse(robotsPageDefinition.CanFollowLinks("NoSpaces/1.0"));
	}

	[TestMethod]
	public void CustomRule()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsFalse(robotsPageDefinition.HasRule(RobotsPageDirectives.NoTranslate, "unlistedbot"));
		Assert.IsTrue(robotsPageDefinition.HasRule(RobotsPageDirectives.NoTranslate, "CustomBot/5.1"));
	}

	[TestMethod]
	public void UnlistedAgentRule()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.HasRule(RobotsPageDirectives.NoArchive, "unlistedbot"));
		Assert.IsFalse(robotsPageDefinition.HasRule(RobotsPageDirectives.NoTranslate, "unlistedbot"));
	}

	[TestMethod]
	public void UnlistedAgentRuleWithValue()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.UnavailableAfter, "unlistedbot", out var value));
		Assert.AreEqual("25 Jun 2010 15:00:00 PST", value);
	}

	[TestMethod]
	public void ListedAgentRuleWithValue()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.MaxSnippet, "RuleWithValue/1.0", out var value));
		Assert.AreEqual("1", value);
	}

	[TestMethod]
	public void ListedAgentWithDuplicateRulesInLineValue()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.MaxSnippet, "DuplicateRulesWithValue/2.1", out var value));
		Assert.AreEqual("4", value);
	}

	[TestMethod]
	public void ListedAgentDirectiveOverride()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("DirectiveOverride-Example.txt");

		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.MaxSnippet, "CustomBot/3.0", out var valueOne));
		Assert.AreEqual("4", valueOne);
		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.UnavailableAfter, "CustomBot/3.0", out var valueTwo));
		Assert.AreEqual("30 Jun 2010 15:00:00 PST", valueTwo);
	}

	[TestMethod]
	public void UnlistedAgentDirectiveOverride()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("DirectiveOverride-Example.txt");

		Assert.IsFalse(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.MaxSnippet, "unlistedbot", out _));
		Assert.IsTrue(robotsPageDefinition.TryGetRuleValue(RobotsPageDirectives.UnavailableAfter, "unlistedbot", out var value));
		Assert.AreEqual("30 Jun 2010 15:00:00 PST", value);
	}

	[TestMethod]
	public void DuplicateDirectiveValues()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("RobotsPage-Example.txt");

		Assert.IsTrue(robotsPageDefinition.TryGetEntryForUserAgent("DuplicateValues/1.0", out var entry));
		Assert.AreEqual(1, entry.Directives.Where(d => d.Name == RobotsPageDirectives.NoIndex).Count());
	}

	[TestMethod]
	public void InvalidRulesAndDirectives()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("InvalidData-Example.txt");

		Assert.AreEqual(0, robotsPageDefinition.PageAccessEntries.Count);
	}

	[TestMethod]
	public void PartiallyInvalidRulesAndDirectives()
	{
		var robotsPageDefinition = GetRobotsPageDefinition("PartiallyInvalidData-Example.txt");

		Assert.AreEqual(1, robotsPageDefinition.PageAccessEntries.Count);
		var entry = robotsPageDefinition.PageAccessEntries.FirstOrDefault();
		Assert.AreEqual(Constants.UserAgentWildcard, entry.UserAgent);
		Assert.AreEqual(1, entry.Directives.Count);
		Assert.AreEqual(RobotsPageDirectives.NoIndex, entry.Directives.FirstOrDefault().Name);
	}
}
