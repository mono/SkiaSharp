using System.IO;
using System.Threading;

namespace SkiaSharp.Fuzzing;

internal static class FuzzSeeds
{
	private static int initialized;

	public static void EnsureInitialized(string fuzzerName)
	{
		if (Interlocked.Exchange(ref initialized, 1) != 0)
			return;

		var inputsDirectory = Path.Combine(Environment.CurrentDirectory, "task_inputs_0");
		if (!Directory.Exists(inputsDirectory))
			return;

		var seedsDirectory = Path.Combine(
			Path.GetDirectoryName(typeof(IFuzzer).Assembly.Location)!,
			"seeds",
			fuzzerName);
		if (!Directory.Exists(seedsDirectory))
			return;

		foreach (var seedFile in Directory.GetFiles(seedsDirectory))
		{
			var destinationFile = Path.Combine(inputsDirectory, Path.GetFileName(seedFile));
			var tempFile = Path.Combine(Environment.CurrentDirectory, $".seed-{Guid.NewGuid():N}.tmp");
			File.Copy(seedFile, tempFile, overwrite: true);
			File.Delete(destinationFile);
			File.Move(tempFile, destinationFile, overwrite: true);
		}
	}
}

internal sealed class FuzzStream : Stream
{
	private readonly byte[] data;
	private readonly bool seekable;
	private long position;
	private bool disposed;

	public FuzzStream(byte[] data, bool seekable)
	{
		this.data = data;
		this.seekable = seekable;
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

		var remaining = data.Length - (int)Math.Min(position, data.Length);
		var count = Math.Min(buffer.Length, remaining);
		if (count == 0)
			return 0;

		data.AsSpan((int)position, count).CopyTo(buffer);
		position += count;
		return count;
	}

	public override int ReadByte()
	{
		ObjectDisposedException.ThrowIf(disposed, this);
		if (position >= data.Length)
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
}

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

