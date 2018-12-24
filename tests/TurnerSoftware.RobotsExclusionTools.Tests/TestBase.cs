﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TurnerSoftware.RobotsExclusionTools.Tests
{
	public class TestBase
	{
		protected string LoadRobotsResource(string name)
		{
			return File.ReadAllText($"Resources/{name}");
		}

		protected string LoadRFCExample()
		{
			return LoadRobotsResource("NoRobots-RFC-Example.txt");
		}

		protected RobotsFile GetRFCRobotsFile()
		{
			var robots = LoadRFCExample();
			var robotsFile = new RobotsParser().FromString(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}
	}
}
