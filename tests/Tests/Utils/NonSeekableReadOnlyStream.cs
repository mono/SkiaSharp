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
		private long position;

		public ConfigurableTestStream(
			byte[] data,
			bool canSeek = true,
			int maxBytesPerRead = int.MaxValue)
		{
			this.data = data ?? throw new ArgumentNullException(nameof(data));
			this.canSeek = canSeek;
			this.maxBytesPerRead = maxBytesPerRead;
		}

		public override bool CanRead => true;
		public override bool CanSeek => canSeek;
		public override bool CanWrite => false;

		public override long Length =>
			canSeek ? data.Length : throw new NotSupportedException();

		public override long Position
		{
			get => canSeek ? position : throw new NotSupportedException();
			set
			{
				if (!canSeek)
					throw new NotSupportedException();

				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value));

				position = value;
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count <= 0)
				return 0;

			if (position >= data.Length)
				return 0;

			var available = (int)(data.Length - position);
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
				SeekOrigin.End => data.Length + offset,
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
}
