using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class RobotsPageDefinitionTests : TestBase
	{
		[TestMethod]
		public void CanIndex()
		{
			var definition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsTrue(definition.CanIndex("GoogleBot/1.0"));
			Assert.IsFalse(definition.CanIndex("OtherBot/3.5"));
			Assert.IsFalse(definition.CanIndex("SomeBot"));
			Assert.IsTrue(definition.CanIndex("SomeUnlistedBot/1.0"));
		}

		[TestMethod]
		public void CanFollowLinks()
		{
			var definition = GetRobotsPageDefinition("RobotsPage-Example.txt");

			Assert.IsFalse(definition.CanFollowLinks("GoogleBot/2.77"));
			Assert.IsFalse(definition.CanFollowLinks("OtherBot/4.2"));
			Assert.IsFalse(definition.CanFollowLinks("SomeBot"));
			Assert.IsTrue(definition.CanFollowLinks("SomeUnlistedBot/1.0"));
		}

		[TestMethod]
		public void LastRuleOverride()
		{
			var definition = GetRobotsPageDefinition("LastRuleOverride-Example.txt");

			Assert.IsTrue(definition.CanIndex("GoogleBot/2.5"));
			Assert.IsTrue(definition.CanFollowLinks("GoogleBot/2.5"));
			Assert.IsFalse(definition.CanIndex("OtherBot/2.0"));
			Assert.IsFalse(definition.CanFollowLinks("OtherBot/2.0"));
			Assert.IsTrue(definition.CanIndex("SomeBot/1.0"));
			Assert.IsFalse(definition.CanFollowLinks("SomeBot/1.0"));
		}
	}
}
