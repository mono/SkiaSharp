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
		var selector = payload[0];
		var preDecode = (selector & 1) != 0;
		var seekable = (selector & 2) == 0;

		using var dataUriProvider = new DataUriResourceProvider(preDecode: preDecode);
		using var cachingProvider = new CachingResourceProvider(dataUriProvider);

		using var stream = new FuzzStream(payload, seekable);
		using var builder = Animation.CreateBuilder();
		using var animation = builder
			.SetResourceProvider(cachingProvider)
			.SetFontManager(SKFontManager.Default)
			.Build(stream);
		if (animation is null)
			return;

		_ = builder.Stats;
		_ = animation.Duration;
		_ = animation.Fps;
		_ = animation.InPoint;
		_ = animation.OutPoint;
		_ = animation.Version;
		_ = animation.Size;

		using var bitmap = new SKBitmap(64, 64);
		using var canvas = new SKCanvas(bitmap);
		using var invalidation = new InvalidationController();

		invalidation.Begin();
		animation.Seek(0.0, invalidation);
		invalidation.End();
		_ = invalidation.Bounds;
		animation.Render(canvas, SKRect.Create(64, 64));

		invalidation.Begin();
		animation.Seek(((selector >> 2) & 0x0f) / 15.0, invalidation);
		invalidation.End();
		_ = invalidation.Bounds;
		animation.Render(canvas, SKRect.Create(64, 64));
	}
}
