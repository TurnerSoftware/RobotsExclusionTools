using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

public static class StreamLineReader
{
	private const byte NewLineChar = (byte)'\n';

	public static IAsyncEnumerable<ReadOnlySequence<byte>> EnumerateLinesAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		var pipe = new Pipe();
		var pipeWriterTask = FillPipeAsync(stream, pipe.Writer, cancellationToken);
		return ReadPipeAsync(pipeWriterTask, pipe.Reader, cancellationToken);
	}

	private static async Task FillPipeAsync(Stream stream, PipeWriter writer, CancellationToken cancellationToken)
	{
		await stream.CopyToAsync(writer, cancellationToken);
		writer.Complete();
	}

	private static async IAsyncEnumerable<ReadOnlySequence<byte>> ReadPipeAsync(
		Task pipeWriterTask, 
		PipeReader reader, 
		[EnumeratorCancellation] CancellationToken cancellationToken
	)
	{
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
		await pipeWriterTask;
	}
}
