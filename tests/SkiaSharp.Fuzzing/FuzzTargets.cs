using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using HarfBuzzSharp;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;
using HBBuffer = HarfBuzzSharp.Buffer;
using HBFont = HarfBuzzSharp.Font;
using HBFace = HarfBuzzSharp.Face;
using HBBlob = HarfBuzzSharp.Blob;

namespace SkiaSharp.Fuzzing;

public static class FuzzTargets
{
	private static readonly Tag[] HarfBuzzTags =
	[
		Tag.Parse("cmap"),
		Tag.Parse("head"),
		Tag.Parse("hhea"),
		Tag.Parse("hmtx"),
		Tag.Parse("name"),
		Tag.Parse("post"),
	];

	private static readonly Lazy<Dictionary<uint, byte[]>> HarfBuzzTableBytes = new(LoadHarfBuzzTableBytes);

	private const long MaximumDecodedBytes = 16 * 1024 * 1024;
	private const int MaximumDimension = 4096;
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	static FuzzTargets()
	{
		var loadContext = AssemblyLoadContext.GetLoadContext(typeof(FuzzTargets).Assembly);
		if (loadContext is not null)
			loadContext.Resolving += ResolveSharpFuzzAssembly;
	}

	public static void FuzzManagedStreams(ReadOnlySpan<byte> input)
	{
		if (input.Length < 2 || input.Length > MaximumInputBytes)
			return;

		var mode = input[0];
		var payload = input[1..].ToArray();

		ExerciseStreams(payload, mode, DecodeImage);
		ExerciseStreams(payload, mode, DecodeBitmap);
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

	public static void FuzzHarfBuzzManagedTableCallbacks(ReadOnlySpan<byte> input)
	{
		if (input.Length < 2 || input.Length > MaximumInputBytes)
			return;

		var mode = input[0];
		var payload = input[1..].ToArray();
		var text = GetFuzzText(payload);

		HBFace? face = null;
		HBFont? font = null;

		try
		{
			face = new HBFace((_, tag) => CreateFuzzTable(tag, payload, mode), () => { });
			font = new HBFont(face);

			using var buffer = new HBBuffer();
			buffer.AddUtf8(text);
			buffer.GuessSegmentProperties();

			InvokeHarfBuzzOperation(mode, () => font.Shape(buffer));
			_ = buffer.Length;

			foreach (var tag in HarfBuzzTags)
			{
				InvokeHarfBuzzOperation(mode, () =>
				{
					using var table = face.ReferenceTable(tag);
					_ = table.Length;
				});
			}
		}
		finally
		{
			if ((mode & 0x04) == 0)
			{
				font?.Dispose();
				face?.Dispose();
			}
			else
			{
				face?.Dispose();
				font?.Dispose();
			}
		}
	}

	public static void FuzzSkottieResourceProviders(ReadOnlySpan<byte> input)
	{
		if (input.Length < 2 || input.Length > MaximumInputBytes)
			return;

		var mode = input[0];
		var payload = input[1..].ToArray();
		var baseDirectory = Path.Combine(AppContext.BaseDirectory, "seed-sources", "images");
		if (!Directory.Exists(baseDirectory))
			return;

		using var fileProvider = new FileResourceProvider(baseDirectory, preDecode: (mode & 0x01) != 0);
		using var dataUriProvider = new DataUriResourceProvider(fileProvider, preDecode: (mode & 0x02) != 0);
		using var cachingProvider = new CachingResourceProvider(dataUriProvider);

		var resourcePath = GetResourcePath(payload);
		var resourceName = GetResourceName(payload);

		if (TryResolveContainedPath(baseDirectory, resourcePath, resourceName, out _))
		{
			using var loaded = cachingProvider.Load(resourcePath, resourceName);
			_ = loaded?.Size ?? 0;
		}

		var dataUri = "data:application/octet-stream;base64," +
			Convert.ToBase64String(payload.AsSpan(0, Math.Min(payload.Length, 512)).ToArray());
		using (var loaded = dataUriProvider.Load(dataUri))
		{
			_ = loaded?.Size ?? 0;
		}

		BuildAndRenderSkottie(Path.Combine(baseDirectory, "LottieLogo1.json"), cachingProvider, mode);
		BuildAndRenderSkottie(Path.Combine(baseDirectory, "lottie-base64_dotnet-bot.json"), cachingProvider, mode);
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

	private static void DecodeBitmap(Stream stream)
	{
		using var bitmap = SKBitmap.Decode(stream);
		if (bitmap is null)
			return;

		if (bitmap.Width <= 0 ||
			bitmap.Height <= 0 ||
			bitmap.Width > MaximumDimension ||
			bitmap.Height > MaximumDimension)
			return;

		_ = bitmap.Bytes.Length;
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

	private static void BuildAndRenderSkottie(string path, ResourceProvider provider, byte mode)
	{
		if (!File.Exists(path))
			return;

		using var data = SKData.Create(path);
		using var animation = Animation
			.CreateBuilder()
			.SetResourceProvider(provider)
			.Build(data);
		if (animation is null)
			return;

		var width = Math.Clamp((int)Math.Ceiling(animation.Size.Width), 1, 512);
		var height = Math.Clamp((int)Math.Ceiling(animation.Size.Height), 1, 512);
		using var bitmap = new SKBitmap(width, height);
		using var canvas = new SKCanvas(bitmap);
		animation.Seek((mode & 0x0f) / 15.0);
		animation.Render(canvas, SKRect.Create(width, height));
	}

	private static void InvokeHarfBuzzOperation(byte mode, Action action)
	{
		if ((mode & 0x80) != 0)
		{
			try
			{
				action();
			}
			catch (InvalidOperationException)
			{
			}

			return;
		}

		action();
	}

	private static HBBlob? CreateFuzzTable(Tag tag, byte[] payload, byte mode)
	{
		if ((mode & 0x20) != 0 && (((uint)tag) & 1u) == 0)
			return null;

		if (!HarfBuzzTableBytes.Value.TryGetValue(tag, out var source))
			return null;

		var length = source.Length;
		if ((mode & 0x40) != 0 && source.Length > 1)
		{
			var cap = payload.Length == 0
				? 1
				: 1 + (payload[0] % Math.Min(source.Length, 255));
			length = Math.Max(1, Math.Min(source.Length, cap));
		}

		var bytes = new byte[length];
		System.Buffer.BlockCopy(source, 0, bytes, 0, length);
		return CreateOwnedBlob(bytes);
	}

	private static HBBlob CreateOwnedBlob(byte[] bytes)
	{
		if (bytes.Length == 0)
			return HBBlob.Empty;

		var data = Marshal.AllocHGlobal(bytes.Length);
		Marshal.Copy(bytes, 0, data, bytes.Length);
		return new HBBlob(data, bytes.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(data));
	}

	private static Dictionary<uint, byte[]> LoadHarfBuzzTableBytes()
	{
		var tables = new Dictionary<uint, byte[]>();
		var fontPath = Path.Combine(AppContext.BaseDirectory, "seed-sources", "ZeroGlyphs.ttf");
		if (!File.Exists(fontPath))
			return tables;

		using var blob = HBBlob.FromFile(fontPath);
		using var face = new HBFace(blob, 0);
		foreach (var tag in face.Tables)
		{
			using var table = face.ReferenceTable(tag);
			var span = table.AsSpan();
			if (span.Length == 0)
				continue;

			tables[(uint)tag] = span.ToArray();
		}

		return tables;
	}

	private static string GetFuzzText(byte[] payload)
	{
		if (payload.Length == 0)
			return "SkiaSharp fuzz";

		var text = Encoding.UTF8.GetString(payload, 0, Math.Min(payload.Length, 128));
		return string.IsNullOrWhiteSpace(text) ? "SkiaSharp fuzz" : text;
	}

	private static string GetResourcePath(byte[] payload)
	{
		if (payload.Length < 3)
			return string.Empty;

		var raw = Encoding.UTF8.GetString(payload, 0, Math.Min(payload.Length / 2, 64)).Trim();
		return string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Replace('\0', '_');
	}

	private static string GetResourceName(byte[] payload)
	{
		if (payload.Length < 4)
			return "baboon.png";

		var start = payload.Length / 2;
		var raw = Encoding.UTF8.GetString(payload, start, Math.Min(payload.Length - start, 64)).Trim();
		return string.IsNullOrWhiteSpace(raw) ? "baboon.png" : raw.Replace('\0', '_');
	}

	private static bool TryResolveContainedPath(string root, string resourcePath, string resourceName, out string fullPath)
	{
		var comparison = OperatingSystem.IsWindows()
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		var normalizedRoot = Path.GetFullPath(root);
		if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar))
			normalizedRoot += Path.DirectorySeparatorChar;

		fullPath = Path.GetFullPath(Path.Combine(root, resourcePath, resourceName));
		return fullPath.StartsWith(normalizedRoot, comparison);
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
