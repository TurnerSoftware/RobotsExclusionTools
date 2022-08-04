using System.Text;
using BenchmarkDotNet.Attributes;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks;

[Config(typeof(IntrinsicBenchmarkConfig))]
public class IsValidPathBenchmark
{
	[Params("/about", "/wwwroot/html/assets/logo.png")]
	public string Path;

	private byte[] PathData;

	[GlobalSetup]
	public void Setup()
	{
		PathData = Encoding.UTF8.GetBytes(Path);
	}

	[Benchmark]
	public bool IsValidPath() => RobotsExclusionProtocolHelper.IsValidPath(PathData);
}
