using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace TurnerSoftware.RobotsExclusionTools.Helpers;

public static class StreamLineReader
{
	private const byte NewLineChar = (byte)'\n';
	private static readonly StreamPipeReaderOptions Options = new(leaveOpen: true);

	public static async IAsyncEnumerable<ReadOnlySequence<byte>> EnumerateLinesOfBytesAsync(
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

	public static async IAsyncEnumerable<ReadOnlyMemory<char>> EnumerateLinesOfCharsAsync(
		Stream stream,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		await foreach (var lineSequence in EnumerateLinesOfBytesAsync(stream, cancellationToken))
		{
			var numberOfBytes = (int)lineSequence.Length;
			var charArray = ArrayPool<char>.Shared.Rent(numberOfBytes);
			try
			{
				var numberOfChars = GetUtf8Chars(lineSequence, charArray);
				yield return charArray.AsMemory(0, numberOfChars);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(charArray);
			}
		}
	}

	private static int GetUtf8Chars(ReadOnlySequence<byte> source, char[] destination)
	{
		var numberOfBytes = (int)source.Length;
		byte[] byteArray = null;
		try
		{
#if NETSTANDARD2_0
			byteArray = ArrayPool<byte>.Shared.Rent(numberOfBytes);
			source.CopyTo(byteArray);
			return Encoding.UTF8.GetChars(byteArray, 0, numberOfBytes, destination, 0);
#else
			if (source.IsSingleSegment)
			{
				return Encoding.UTF8.GetChars(source.FirstSpan, destination);
			}
			else
			{
				byteArray = ArrayPool<byte>.Shared.Rent(numberOfBytes);
				source.CopyTo(byteArray);
				return Encoding.UTF8.GetChars(byteArray, destination);
			}
#endif
		}
		finally
		{
			if (byteArray is not null)
			{
				ArrayPool<byte>.Shared.Return(byteArray);
			}
		}
	}
}
