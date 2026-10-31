using System;
using System.IO;

namespace SkiaSharp.Tests
{
	public class NonSeekableWriteOnlyStream : Stream
	{
		private readonly Stream stream;

		public NonSeekableWriteOnlyStream()
			: this(new MemoryStream())
		{
		}

		public NonSeekableWriteOnlyStream(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		public byte[] ToArray() =>
			stream is MemoryStream ms
				? ms.ToArray()
				: throw new NotSupportedException();

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => stream.CanWrite;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override void Flush() => stream.Flush();

		public override int Read(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin) =>
			throw new NotSupportedException();

		public override void SetLength(long value) =>
			throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) =>
			stream.Write(buffer, offset, count);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				stream.Dispose();
			base.Dispose(disposing);
		}
	}
}
