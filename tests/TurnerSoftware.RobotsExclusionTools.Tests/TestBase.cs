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
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			TestConfiguration.StartupServer();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			TestConfiguration.ShutdownServer();
		}

		protected string LoadResource(string name)
		{
			return File.ReadAllText($"Resources/{name}");
		}

		protected string LoadRFCExample()
		{
			return LoadResource("RobotsFile/NoRobots-RFC-Example.txt");
		}

		protected RobotsFile GetRobotsFile(string name)
		{
			var robots = LoadResource("RobotsFile/" + name);
			var robotsFile = new RobotsParser().FromString(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}

		protected RobotsFile GetRFCRobotsFile()
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
