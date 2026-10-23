namespace SkiaSharp.Tests.Shared;

public sealed class GoldenImageSource
{
	private readonly Func<byte[]> read;

	private GoldenImageSource(string location, Func<byte[]> read)
	{
		Location = location;
		this.read = read;
	}

	public string Location { get; }

	public byte[] ReadBytes() => read();

	public static implicit operator GoldenImageSource(string path) => FromFile(path);

	public static GoldenImageSource FromBytes(byte[] bytes, string location)
	{
		if (bytes == null) throw new ArgumentNullException(nameof(bytes));
		if (location == null) throw new ArgumentNullException(nameof(location));
		return new GoldenImageSource(location, () => bytes);
	}

	public static GoldenImageSource FromFile(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		return new GoldenImageSource(path, () => File.ReadAllBytes(path));
	}

	public static GoldenImageSource WritePng(byte[] bytes, string path)
	{
		if (bytes == null) throw new ArgumentNullException(nameof(bytes));
		if (path == null) throw new ArgumentNullException(nameof(path));
		GoldenImage.Write(path, bytes);
		if (!GoldenImage.HasPngSignature(bytes))
			throw new InvalidOperationException("Expected a PNG image.");
		return FromFile(path);
	}
}
