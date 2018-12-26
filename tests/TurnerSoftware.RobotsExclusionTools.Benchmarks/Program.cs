using System;
using BenchmarkDotNet.Running;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<StreamingVsStringTokenization>();
		}
	}
}
