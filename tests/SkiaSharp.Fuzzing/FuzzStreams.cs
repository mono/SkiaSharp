using System.IO;

namespace SkiaSharp.Fuzzing;

internal sealed class FuzzStream : Stream
{
	private readonly byte[] data;
	private readonly bool seekable;
	private readonly int maximumReadLength;
	private readonly int earlyEndPosition;
	private long position;

	public FuzzStream(byte[] data, byte mode, bool seekable)
	{
		this.data = data;
		this.seekable = seekable;
		maximumReadLength = (mode & 0x40) == 0
			? int.MaxValue
			: 1 + (mode & 0x0f);
		earlyEndPosition = (mode & 0x80) == 0
			? -1
			: data.Length == 0
				? 0
				: (int)((long)data.Length * (1 + (mode & 0x3f)) / 64);
	}

	public override bool CanRead => true;

	public override bool CanSeek => seekable;

	public override bool CanWrite => false;

	public override long Length =>
		seekable ? data.Length : throw new NotSupportedException();

	public override long Position
	{
		get => seekable ? position : throw new NotSupportedException();
		set
		{
			if (!seekable)
				throw new NotSupportedException();
			if (value < 0 || value > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(value));

			position = value;
		}
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count) =>
		Read(buffer.AsSpan(offset, count));

	public override int Read(Span<byte> buffer)
	{
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
		if (HasReachedEarlyEnd() || position >= data.Length)
			return -1;

		return data[(int)position++];
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
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

	private bool HasReachedEarlyEnd() =>
		earlyEndPosition >= 0 && position >= earlyEndPosition;
}
