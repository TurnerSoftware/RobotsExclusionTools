using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks
{
	[ClrJob]
	[MemoryDiagnoser]
	public class StreamingVsStringTokenization
	{
		private RobotsParser Parser { get; }
		private Uri Uri { get; }

		public StreamingVsStringTokenization()
		{
			Parser = new RobotsParser();
			Uri = new Uri("https://www.google.com");
		}

		[Benchmark]
		public async Task StreamingTokenization()
		{
			using (var fileStream = new FileStream("Resources/Google-Robots.txt", FileMode.Open))
			{
				var result = await Parser.FromStreamAsync(fileStream, Uri);
			}
		}

		[Benchmark]
		public async Task StringTokenization()
		{
			string robots;

			using (var fileStream = new FileStream("Resources/Google-Robots.txt", FileMode.Open))
			{
				using (var streamReader = new StreamReader(fileStream))
				{
					robots = await streamReader.ReadToEndAsync();
				}
			}
			
			var result = Parser.FromString(robots, Uri);
		}
	}
}
