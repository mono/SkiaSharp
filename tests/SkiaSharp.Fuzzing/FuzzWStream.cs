using System.IO;

namespace SkiaSharp.Fuzzing;

/// <summary>
/// Write-only sink <see cref="Stream"/> that tracks position and length without allocating a
/// backing buffer, for exercising <see cref="SKManagedWStream"/> in seekable and non-seekable modes.
/// </summary>
internal sealed class FuzzWStream : Stream
{
	private readonly bool seekable;
	private long position;
	private long length;
	private bool disposed;

	public FuzzWStream(bool seekable)
	{
		this.seekable = seekable;
	}

	public override bool CanRead => false;

	public override bool CanSeek => !disposed && seekable;

	public override bool CanWrite => !disposed;

	public override long Length
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return seekable ? length : throw new NotSupportedException();
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
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value));

			position = value;
		}
	}

	public override void Flush() =>
		ObjectDisposedException.ThrowIf(disposed, this);

	public override int Read(byte[] buffer, int offset, int count) =>
		throw new NotSupportedException();

	public override long Seek(long offset, SeekOrigin origin)
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (!seekable)
			throw new NotSupportedException();

		var target = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => checked(position + offset),
			SeekOrigin.End => checked(length + offset),
			_ => throw new ArgumentOutOfRangeException(nameof(origin)),
		};

		Position = target;
		return position;
	}

	public override void SetLength(long value)
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (!seekable)
			throw new NotSupportedException();
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value));

		length = value;
		if (position > length)
			position = length;
	}

	public override void Write(byte[] buffer, int offset, int count) =>
		Write(buffer.AsSpan(offset, count));

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		position += buffer.Length;
		if (position > length)
			length = position;
	}

	protected override void Dispose(bool disposing)
	{
		disposed = true;
		base.Dispose(disposing);
	}
}
