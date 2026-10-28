using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class NonSeekableReadOnlyStream : Stream
	{
		private readonly Stream stream;

		public NonSeekableReadOnlyStream(Stream stream)
		{
			this.stream = stream;
		}

		public override bool CanRead => stream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get { return stream.Position; }
			set { throw new NotSupportedException(); }
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}

	public sealed class ConfigurableTestStream : Stream
	{
		private readonly byte[] data;
		private readonly bool canSeek;
		private readonly int maxBytesPerRead;
		private readonly int earlyEndPosition;
		private readonly long reportedLength;
		private readonly int maxZeroReadsAfterEof;
		private long position;
		private int consecutiveZeroReads;

		public ConfigurableTestStream(
			byte[] data,
			bool canSeek = true,
			int maxBytesPerRead = int.MaxValue,
			int earlyEndPosition = -1,
			long? reportedLength = null,
			int maxZeroReadsAfterEof = 16)
		{
			this.data = data ?? throw new ArgumentNullException(nameof(data));
			this.canSeek = canSeek;
			this.maxBytesPerRead = maxBytesPerRead;
			this.earlyEndPosition = earlyEndPosition;
			this.reportedLength = reportedLength ?? data.Length;
			this.maxZeroReadsAfterEof = maxZeroReadsAfterEof;
		}

		public int TotalReadCalls { get; private set; }
		public int ZeroReadCount { get; private set; }
		public int MaxRequestedReadCount { get; private set; }
		public bool InfiniteLoopDetected { get; private set; }
		public bool NegativePositionAttempted { get; private set; }

		public override bool CanRead => true;
		public override bool CanSeek => canSeek;
		public override bool CanWrite => false;

		public override long Length =>
			canSeek ? reportedLength : throw new NotSupportedException();

		public override long Position
		{
			get => canSeek ? position : throw new NotSupportedException();
			set
			{
				if (!canSeek)
					throw new NotSupportedException();

				consecutiveZeroReads = 0;
				if (value < 0)
				{
					NegativePositionAttempted = true;
					position = 0;
					return;
				}

				position = value;
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			TotalReadCalls++;
			if (count > MaxRequestedReadCount)
				MaxRequestedReadCount = count;

			if (count <= 0)
				return 0;

			var logicalEnd = earlyEndPosition >= 0
				? Math.Min(data.Length, earlyEndPosition)
				: data.Length;

			if (position >= logicalEnd)
			{
				ZeroReadCount++;
				consecutiveZeroReads++;
				if (consecutiveZeroReads > maxZeroReadsAfterEof)
				{
					InfiniteLoopDetected = true;
					position = Math.Max(position, reportedLength);
				}
				return 0;
			}

			consecutiveZeroReads = 0;
			var available = (int)Math.Min(logicalEnd - position, int.MaxValue);
			var toCopy = Math.Min(Math.Min(count, maxBytesPerRead), available);
			Buffer.BlockCopy(data, (int)position, buffer, offset, toCopy);
			position += toCopy;
			return toCopy;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!canSeek)
				throw new NotSupportedException();

			var target = origin switch
			{
				SeekOrigin.Begin => offset,
				SeekOrigin.Current => position + offset,
				SeekOrigin.End => reportedLength + offset,
				_ => throw new ArgumentOutOfRangeException(nameof(origin)),
			};

			Position = target;
			return position;
		}

		public override void SetLength(long value) =>
			throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException();
	}

	public sealed class HostileReadReturnStream : Stream
	{
		private readonly int returnValue;

		public HostileReadReturnStream(int returnValue)
		{
			this.returnValue = returnValue;
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => 100;
		public override long Position { get; set; }
		public override void Flush() { }

		public override int Read(byte[] buffer, int offset, int count)
		{
			// Fill buffer with 0xAA up to buffer.Length so we can detect if caller copies more than `count`
			for (var i = offset; i < buffer.Length; i++)
				buffer[i] = 0xAA;

			return returnValue == int.MaxValue ? count + 16 : returnValue;
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		public override int Read(Span<byte> buffer)
		{
			buffer.Fill(0xAA);
			return returnValue == int.MaxValue ? buffer.Length + 16 : returnValue;
		}
#endif

		public override long Seek(long offset, SeekOrigin origin) => Position = offset;
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
