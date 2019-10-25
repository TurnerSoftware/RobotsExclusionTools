using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace TurnerSoftware.RobotsExclusionTools.Benchmarks
{
	[SimpleJob(RuntimeMoniker.NetCoreApp21), SimpleJob(RuntimeMoniker.Net461, baseline: true)]
	[MemoryDiagnoser]
	public class StreamingVsStringTokenization
	{
		private RobotsFileParser Parser { get; }
		private Uri Uri { get; }

		private MemoryStream MemoryStream { get; }
		private string RobotsText { get; }

		public StreamingVsStringTokenization()
		{
			Parser = new RobotsFileParser();
			Uri = new Uri("https://www.google.com");

			MemoryStream = new MemoryStream();
			
			using (var fileStream = new FileStream("Resources/Google-Robots.txt", FileMode.Open))
			{
				fileStream.CopyTo(MemoryStream);
			}

			var streamReader = new StreamReader(MemoryStream);
			RobotsText = streamReader.ReadToEnd();
			MemoryStream.Seek(0, SeekOrigin.Begin);
		}

		[Benchmark]
		public async Task MemoryStreamingTokenization()
		{
			var result = await Parser.FromStreamAsync(MemoryStream, Uri);
			MemoryStream.Seek(0, SeekOrigin.Begin);
		}

		[Benchmark]
		public async Task MemoryStringTokenization()
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

		[Benchmark]
		public async Task FileStreamingTokenization()
		{
			using (var fileStream = new FileStream("Resources/Google-Robots.txt", FileMode.Open))
			{
				var result = await Parser.FromStreamAsync(fileStream, Uri);
			}
		}

		[Benchmark]
		public async Task FileStringTokenization()
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
