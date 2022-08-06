using System;
using BenchmarkDotNet.Attributes;
using TurnerSoftware.RobotsExclusionTools.Helpers;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks;

[Config(typeof(DefaultBenchmarkConfig))]
public class PathMatchingBenchmark
{
	private const string UriPath = "/foo/bar/baz";

	[Benchmark]
	public bool Simple() => PathComparisonUtility.PathMatch("/foo/bar/baz", UriPath, StringComparison.InvariantCulture);

	[Benchmark]
	public bool Wildcard() => PathComparisonUtility.PathMatch("/foo/*/baz", UriPath, StringComparison.InvariantCulture);

	[Benchmark]
	public bool MustEndWith() => PathComparisonUtility.PathMatch("/foo/bar/baz$", UriPath, StringComparison.InvariantCulture);
}
