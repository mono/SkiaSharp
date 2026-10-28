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
}
