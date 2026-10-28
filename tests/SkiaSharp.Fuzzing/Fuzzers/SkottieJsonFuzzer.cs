using System.IO;
using SkiaSharp.Resources;
using SkiaSharp.SceneGraph;
using SkiaSharp.Skottie;

namespace SkiaSharp.Fuzzing.Fuzzers;

/// <summary>
/// Fuzzes Lottie JSON animations (<see cref="AnimationBuilder.Build(Stream)" /> and <see cref="Animation" />) over
/// seekable and non-seekable managed streams (<see cref="SKData.Create(Stream)" /> and preamble stripping), embedded
/// data-URI resource loading (<see cref="DataUriResourceProvider" /> and <see cref="CachingResourceProvider" />),
/// <see cref="InvalidationController" /> bounds tracking, and frame seeking/rendering.
/// </summary>
public sealed class SkottieJsonFuzzer : IFuzzer
{
	private const int MaximumInputBytes = 1 * 1024 * 1024;

	public static string Name => "skottie-json";

	public static void FuzzTarget(ReadOnlySpan<byte> bytes)
	{
		if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
			return;

		var payload = bytes.ToArray();

		Fuzz(payload, seekable: true, preDecode: false);
		Fuzz(payload, seekable: false, preDecode: true);
	}

	private static void Fuzz(byte[] payload, bool seekable, bool preDecode)
	{
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

		var stats = builder.Stats;
		_ = stats.TotalLoadTime;
		_ = stats.JsonSize;

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
