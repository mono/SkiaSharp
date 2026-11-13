using System;
using System.IO;

namespace SkiaSharp.Tests
{
	public sealed class ThrowingWriteStream : MemoryStream
	{
		private readonly bool throwOnWrite;
		private readonly bool throwOnFlush;

		public ThrowingWriteStream(bool throwOnWrite = true, bool throwOnFlush = true)
		{
			this.throwOnWrite = throwOnWrite;
			this.throwOnFlush = throwOnFlush;
		}

		public override void Flush()
		{
			if (throwOnFlush)
				throw new IOException("Simulated flush error.");

			base.Flush();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (throwOnWrite)
				throw new IOException("Simulated write error.");

			base.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			if (throwOnWrite)
				throw new IOException("Simulated write error.");

			base.WriteByte(value);
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		public override void Write(ReadOnlySpan<byte> buffer)
		{
			if (throwOnWrite)
				throw new IOException("Simulated write error.");

			base.Write(buffer);
		}
#endif
	}
}
