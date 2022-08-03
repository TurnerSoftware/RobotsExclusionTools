using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

public static class StreamLineReader
{
	private const byte NewLineChar = (byte)'\n';
	private static readonly StreamPipeReaderOptions Options = new(leaveOpen: true);

	public static async IAsyncEnumerable<ReadOnlySequence<byte>> EnumerateLinesAsSequenceAsync(
		Stream stream,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		var reader = PipeReader.Create(stream, Options);

		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken);
			var buffer = result.Buffer;
			SequencePosition? position;
			do
			{
				position = buffer.PositionOf(NewLineChar);

				if (position != null)
				{
					yield return buffer.Slice(0, position.Value);
					buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
				}
			}
			while (position != null);

			reader.AdvanceTo(buffer.Start, buffer.End);

			if (result.IsCompleted)
			{
				if (!buffer.IsEmpty)
				{
					yield return buffer;
				}
				break;
			}
		}

		reader.Complete();
	}
	public static async IAsyncEnumerable<ReadOnlyMemory<byte>> EnumerateLinesAsMemoryAsync(
		Stream stream,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		await foreach (var lineSequence in EnumerateLinesAsSequenceAsync(stream, cancellationToken))
		{
			var numberOfBytes = (int)lineSequence.Length;
			var byteArray = ArrayPool<byte>.Shared.Rent(numberOfBytes);
			try
			{
				lineSequence.CopyTo(byteArray);
				yield return byteArray.AsMemory(0, numberOfBytes);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(byteArray);
			}
		}
	}
}
