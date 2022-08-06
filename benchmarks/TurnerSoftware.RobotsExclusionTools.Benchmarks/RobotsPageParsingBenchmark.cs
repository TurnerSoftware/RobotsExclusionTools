using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks;

[Config(typeof(DefaultBenchmarkConfig))]
public class RobotsPageParsingBenchmark
{
	private RobotsPageParser Parser { get; } = new RobotsPageParser();

	private IEnumerable<string> Rules { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		using var fileStream = new FileStream("Resources/RobotsPage-Example.txt", FileMode.Open);
		using var streamReader = new StreamReader(fileStream);
		Rules = streamReader.ReadToEnd().Split('\r', '\n');
	}

	[Benchmark]
	public RobotsPageDefinition FromRules()
	{
		return Parser.FromRules(Rules);
	}
}
