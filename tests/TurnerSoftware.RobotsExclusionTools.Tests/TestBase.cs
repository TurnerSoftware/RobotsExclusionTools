using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	[TestClass]
	public class TestBase
	{
		protected string LoadResource(string name)
		{
			return File.ReadAllText($"Resources/{name}");
		}

		protected string LoadRobotsRfcFileExample()
		{
			return LoadResource("RobotsFile/NoRobots-RFC-Example.txt");
		}

		protected RobotsExclusionTools.RobotsFile GetRobotsFile(string name)
		{
			var robots = LoadResource("RobotsFile/" + name);
			var robotsFile = new RobotsFileParser().FromString(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}

		protected RobotsExclusionTools.RobotsFile GetRfcRobotsFile()
		{
			return GetRobotsFile("NoRobots-RFC-Example.txt");
		}
		
		protected RobotsPageDefinition GetRobotsPageDefinition(string name)
		{
			var robots = LoadResource("RobotsPage/" + name);
			var robotsPage = new RobotsPageParser().FromRules(robots.Split('\n'));
			return robotsPage;
		}
	}
}
