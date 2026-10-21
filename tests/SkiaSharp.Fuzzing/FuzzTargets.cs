using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SkiaSharp.Fuzzing;

public static class FuzzTargets
{
	private const long MaximumDecodedBytes = 16 * 1024 * 1024;
	private const int MaximumDimension = 4096;

	static FuzzTargets()
	{
		var loadContext = AssemblyLoadContext.GetLoadContext(typeof(FuzzTargets).Assembly);
		if (loadContext is not null)
			loadContext.Resolving += ResolveSharpFuzzAssembly;
	}

	public static void FuzzManagedStreams(ReadOnlySpan<byte> input)
	{
		if (input.Length < 2)
			return;

		var mode = input[0];
		var payload = input[1..].ToArray();

		ExerciseStreams(payload, mode, DecodeImage);
		ExerciseStreams(payload, mode, DeserializePicture);
		ExerciseStreams(payload, mode, CreateTypeface);
	}

	public static void FuzzPictureDeserialize(ReadOnlySpan<byte> input)
	{
		if (input.IsEmpty)
			return;

		using (var picture = SKPicture.Deserialize(input))
			ExercisePicture(picture);

		using (var data = SKData.CreateCopy(input))
		using (var picture = SKPicture.Deserialize(data))
			ExercisePicture(picture);

		using var stream = new MemoryStream(input.ToArray(), writable: false);
		using var streamPicture = SKPicture.Deserialize(stream);
		ExercisePicture(streamPicture);
	}

	private static void ExerciseStreams(byte[] payload, byte mode, Action<Stream> action)
	{
		using (var stream = new FuzzStream(payload, mode, seekable: true))
			action(stream);

		using (var stream = new FuzzStream(payload, mode, seekable: false))
			action(stream);
	}

	private static void DecodeImage(Stream stream)
	{
		using var codec = SKCodec.Create(stream);
		if (codec is null)
			return;

		var info = codec.Info;
		if (info.Width <= 0 ||
			info.Height <= 0 ||
			info.Width > MaximumDimension ||
			info.Height > MaximumDimension ||
			info.BytesSize64 <= 0 ||
			info.BytesSize64 > MaximumDecodedBytes)
			return;

		using var bitmap = new SKBitmap();
		if (!bitmap.TryAllocPixels(info))
			return;

		_ = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
	}

	private static void DeserializePicture(Stream stream)
	{
		using var picture = SKPicture.Deserialize(stream);
		ExercisePicture(picture);
	}

	private static void CreateTypeface(Stream stream)
	{
		using var typeface = SKFontManager.Default.CreateTypeface(stream);
	}

	private static void ExercisePicture(SKPicture? picture)
	{
		if (picture is null)
			return;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(SKColors.Transparent);
		canvas.DrawPicture(picture);
		canvas.Flush();
	}

	private static Assembly? ResolveSharpFuzzAssembly(AssemblyLoadContext loadContext, AssemblyName assemblyName)
	{
		if (assemblyName.Name != "SharpFuzz.Common")
			return null;

		var path = Path.Combine(AppContext.BaseDirectory, "SharpFuzz.Common.dll");
		return File.Exists(path)
			? loadContext.LoadFromAssemblyPath(path)
			: null;
	}

}
