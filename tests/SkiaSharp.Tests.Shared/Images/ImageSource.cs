namespace SkiaSharp.Tests.Shared;

public sealed class ImageSource
{
	private readonly Func<byte[]> read;

	private ImageSource(string location, Func<byte[]> read)
	{
		Location = location;
		this.read = read;
	}

	public string Location { get; }

	public byte[] ReadBytes() => read();

	public static ImageSource FromBytes(byte[] bytes, string location)
	{
		if (bytes == null) throw new ArgumentNullException(nameof(bytes));
		if (location == null) throw new ArgumentNullException(nameof(location));
		return new ImageSource(location, () => bytes);
	}

	public static ImageSource FromFile(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		return new ImageSource(path, () => File.ReadAllBytes(path));
	}
}
