using System.IO;

namespace SkiaSharp.Fuzzing;

/// <summary>
/// Read-only in-memory <see cref="Stream"/> over fuzzer input bytes that can act as either a
/// seekable stream (<see cref="SKManagedStream"/>) or a non-seekable stream
/// (<see cref="SKFrontBufferedManagedStream"/>).
/// </summary>
internal sealed class FuzzStream : Stream
{
	private readonly byte[] data;
	private readonly bool seekable;
	private readonly int maximumReadLength;
	private readonly int earlyEndPosition;
	private long position;
	private bool disposed;

	public FuzzStream(byte[] data, bool seekable, int maximumReadLength = int.MaxValue, int earlyEndPosition = -1)
	{
		this.data = data;
		this.seekable = seekable;
		this.maximumReadLength = maximumReadLength;
		this.earlyEndPosition = earlyEndPosition;
	}

	public override bool CanRead => !disposed;

	public override bool CanSeek => !disposed && seekable;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return seekable ? data.Length : throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return seekable ? position : throw new NotSupportedException();
		}
		set
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			if (!seekable)
				throw new NotSupportedException();
			if (value < 0 || value > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(value));

			position = value;
		}
	}

	public override void Flush() =>
		ObjectDisposedException.ThrowIf(disposed, this);

	public override int Read(byte[] buffer, int offset, int count) =>
		Read(buffer.AsSpan(offset, count));

	public override int Read(Span<byte> buffer)
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (HasReachedEarlyEnd())
			return 0;

		var logicalLength = earlyEndPosition >= 0
			? Math.Min(data.Length, earlyEndPosition)
			: data.Length;
		var remaining = logicalLength - (int)Math.Min(position, logicalLength);
		var count = Math.Min(Math.Min(buffer.Length, maximumReadLength), remaining);
		if (count == 0)
			return 0;

		data.AsSpan((int)position, count).CopyTo(buffer);
		position += count;
		return count;
	}

	public override int ReadByte()
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (HasReachedEarlyEnd() || position >= data.Length)
			return -1;

		return data[(int)position++];
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (!seekable)
			throw new NotSupportedException();

		var target = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => checked(position + offset),
			SeekOrigin.End => checked(data.Length + offset),
			_ => throw new ArgumentOutOfRangeException(nameof(origin)),
		};

		Position = target;
		return position;
	}

	public override void SetLength(long value) =>
		throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) =>
		throw new NotSupportedException();

	protected override void Dispose(bool disposing)
	{
		disposed = true;
		base.Dispose(disposing);
	}

	private bool HasReachedEarlyEnd() =>
		earlyEndPosition >= 0 && position >= earlyEndPosition;
}
