using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.RobotsExclusionTools.Tests;

[TestClass]
public class TestBase
{
	private static readonly RobotsFileParserMethod[] Methods = new[]
	{
		RobotsFileParserMethod.FromString,
		RobotsFileParserMethod.FromStream
	};

	protected static string LoadResource(string name)
	{
		return File.ReadAllText($"Resources/{name}");
	}
	protected static Stream LoadResourceStream(string name)
	{
		return File.OpenRead($"Resources/{name}");
	}

	protected static async ValueTask WithRobotsFileAsync(string name, Action<RobotsExclusionTools.RobotsFile> action)
	{
		foreach (var method in Methods)
		{
			try
			{
				var robotsFile = await GetRobotsFileAsync(name, method);
				action(robotsFile);
			}
			catch
			{
				Console.WriteLine($"Method: {method}");
				throw;
			}
		}
	}

	protected static ValueTask WithSampleRobotsFileAsync(string domainName, Action<RobotsExclusionTools.RobotsFile> action)
	{
		return WithRobotsFileAsync($"Samples/{domainName}.txt", action);
	}

	private static async ValueTask<RobotsExclusionTools.RobotsFile> GetRobotsFileAsync(string name, RobotsFileParserMethod method)
	{
		if (method is RobotsFileParserMethod.FromString)
		{
			var robots = LoadResource("RobotsFile/" + name);
			var robotsFile = new RobotsFileParser().FromString(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}
		else
		{
			using var robots = LoadResourceStream("RobotsFile/" + name);
			var robotsFile = await new RobotsFileParser().FromStreamAsync(robots, new Uri("http://www.example.org"));
			return robotsFile;
		}
	}
	
	protected static RobotsPageDefinition GetRobotsPageDefinition(string name)
	{
		var robots = LoadResource("RobotsPage/" + name);
		var robotsPage = new RobotsPageParser().FromRules(robots.Split('\r', '\n'));
		return robotsPage;
	}
}

public enum RobotsFileParserMethod
{
	FromString,
	FromStream
}