using System;
using System.IO;

namespace SkiaSharp.Tests
{
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
