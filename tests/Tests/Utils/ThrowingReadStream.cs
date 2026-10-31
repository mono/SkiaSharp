using System;
using System.IO;

namespace SkiaSharp.Tests
{
	public sealed class ThrowingReadStream : Stream
	{
		private readonly long length;
		private bool disposed;

		public ThrowingReadStream(long length = 4096)
		{
			this.length = length;
		}

		public override bool CanRead => !disposed;

		public override bool CanSeek => !disposed;

		public override bool CanWrite => false;

		public override long Length => length;

		public override long Position { get; set; }

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count) =>
			throw new IOException("Simulated read error.");

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		public override int Read(Span<byte> buffer) =>
			throw new IOException("Simulated read error.");
#endif

		public override long Seek(long offset, SeekOrigin origin) =>
			Position = offset;

		public override void SetLength(long value) =>
			throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException();

		protected override void Dispose(bool disposing)
		{
			disposed = true;
			base.Dispose(disposing);
		}
	}
}
