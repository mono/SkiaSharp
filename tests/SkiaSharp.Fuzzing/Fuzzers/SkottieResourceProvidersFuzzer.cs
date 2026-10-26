using System.IO;
using System.Text;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class SkottieResourceProvidersFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "skottie-resource-providers";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();
		var baseDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "images");
		if (!Directory.Exists(baseDirectory))
			return;

		TestProviders(baseDirectory, payload, preDecode: false);
		TestProviders(baseDirectory, payload, preDecode: true);
	}

	private static void TestProviders(string baseDirectory, byte[] payload, bool preDecode)
	{
		using var fileProvider = new FileResourceProvider(baseDirectory, preDecode: preDecode);
		using var dataUriProvider = new DataUriResourceProvider(fileProvider, preDecode: preDecode);
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

		BuildAndRenderSkottie(Path.Combine(baseDirectory, "LottieLogo1.json"), cachingProvider, payload[0]);
		BuildAndRenderSkottie(Path.Combine(baseDirectory, "lottie-base64_dotnet-bot.json"), cachingProvider, payload[0]);
	}

	private static void BuildAndRenderSkottie(string path, ResourceProvider provider, byte frameByte)
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
		animation.Seek((frameByte & 0x0f) / 15.0);
		animation.Render(canvas, SKRect.Create(width, height));
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
}
