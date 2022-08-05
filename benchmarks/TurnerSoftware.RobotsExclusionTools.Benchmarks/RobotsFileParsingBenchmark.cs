using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks;

[Config(typeof(DefaultBenchmarkConfig))]
public class RobotsFileParsingBenchmark
{
	private RobotsFileParser Parser { get; } = new RobotsFileParser();
	private Uri Uri { get; } = new Uri("https://google.com");

	private MemoryStream MemoryStream { get; set; }
	private string RobotsText { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		MemoryStream = new MemoryStream();

		using (var fileStream = new FileStream("Resources/Google-Robots.txt", FileMode.Open))
		{
			fileStream.CopyTo(MemoryStream);
		}

		MemoryStream.Seek(0, SeekOrigin.Begin);
		var streamReader = new StreamReader(MemoryStream);
		RobotsText = streamReader.ReadToEnd();
		MemoryStream.Seek(0, SeekOrigin.Begin);
	}

	[Benchmark]
	public async Task<RobotsFile> FromStream()
	{
		var result = await Parser.FromStreamAsync(MemoryStream, Uri);
		MemoryStream.Seek(0, SeekOrigin.Begin);
		return result;
	}

	[Benchmark]
	public RobotsFile FromString()
	{
		return Parser.FromString(RobotsText, Uri);
	}
}
