using System.IO;
using SkiaSharp.Resources;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;

namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class SkottieJsonFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "skottie-json";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		Fuzz(payload, preDecode: false);
		Fuzz(payload, preDecode: true);
	}

	private static void Fuzz(byte[] payload, bool preDecode)
	{
		using var dataUriProvider = new DataUriResourceProvider(preDecode: preDecode);
		using var cachingProvider = new CachingResourceProvider(dataUriProvider);

		using var data = SKData.CreateCopy(payload);
		using var builder = Animation.CreateBuilder();
		using var animation = builder
			.SetResourceProvider(cachingProvider)
			.SetFontManager(SKFontManager.Default)
			.Build(data);
		if (animation is null)
			return;

		_ = builder.Stats;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		using var invalidation = new InvalidationController();

		foreach (var t in (ReadOnlySpan<double>)[0.0, 0.5, 1.0])
		{
			invalidation.Begin();
			animation.Seek(t, invalidation);
			invalidation.End();
			_ = invalidation.Bounds;
			animation.Render(canvas, SKRect.Create(64, 64));
		}
	}
}
