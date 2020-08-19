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
		protected static string LoadResource(string name)
		{
			return File.ReadAllText($"Resources/{name}");
		}
		protected static Stream LoadResourceStream(string name)
		{
			return File.OpenRead($"Resources/{name}");
		}

		protected static string LoadRobotsRfcFileExample()
		{
			return LoadResource("RobotsFile/NoRobots-RFC-Example.txt");
		}

		protected static RobotsExclusionTools.RobotsFile GetRobotsFile(string name)
		{
			var robots = LoadResource("RobotsFile/" + name);
			var robotsFile = new RobotsFileParser().FromString(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}

		protected static RobotsExclusionTools.RobotsFile GetSampleRobotsFile(string domainName)
		{
			var robots = LoadResource($"RobotsFile/Samples/{domainName}.txt");
			var robotsFile = new RobotsFileParser().FromString(robots, new Uri($"http://{domainName}"));
			return robotsFile;
		}

		protected static RobotsExclusionTools.RobotsFile GetRfcRobotsFile()
		{
			return GetRobotsFile("NoRobots-RFC-Example.txt");
		}
		
		protected static RobotsPageDefinition GetRobotsPageDefinition(string name)
		{
			var robots = LoadResource("RobotsPage/" + name);
			var robotsPage = new RobotsPageParser().FromRules(robots.Split('\n'));
			return robotsPage;
		}
	}
}
