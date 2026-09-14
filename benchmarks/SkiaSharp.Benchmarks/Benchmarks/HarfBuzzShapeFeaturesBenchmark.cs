using System;
using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using HarfBuzzSharp;
using HBBuffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Benchmarks;

// HarfBuzzSharp.Font.Shape(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string>) is the shaping
// entry point used for OpenType-feature-driven text (ligatures, kerning, small-caps, tabular
// figures). It runs once per shaped run / per frame for any text that requests features, and the
// convenience `Shape(Buffer, params Feature[] features)` overload always arrives here with a real
// Feature[] instance.
//
// The previous ("Old") implementation called `features?.ToArray()` unconditionally, allocating a
// fresh Feature[] copy on every shape call even when the caller already owns an array (the common
// `params Feature[]` path). hb_shape_full only *reads* the features (const hb_feature_t*), so that
// defensive copy is pure managed waste. The shipped ("New") implementation pins the caller's array
// directly when `features is Feature[]`, falling back to ToArray() only for non-array lists — the
// pinned pointer and count handed to native are identical either way, so the shaped result is
// unchanged.
//
// This benchmark drives the SAME fixed Font.Shape overload; the only difference between New and Old
// is the concrete list type handed in. New passes the Feature[] directly (the shipped code now pins
// it with no copy); Old wraps the same features in a non-array IReadOnlyList so the fixed code still
// falls back to ToArray() — reproducing the previous per-call allocation. The win shows up first in
// the Allocated column (Feature[] copy -> 0 B). One reused Buffer is reset with ClearContents()
// (a native reset, no managed allocation) between shapes so we measure the marshalling, not buffer
// growth. Feature counts cover a single feature and realistic bundles.
[Config(typeof(Config))]
[MemoryDiagnoser]
public class HarfBuzzShapeFeaturesBenchmark
{
	private class Config : ManualConfig
	{
		public Config() =>
			AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
	}

	[Params(1, 4, 8)]
	public int FeatureCount { get; set; }

	private Blob blob;
	private Face face;
	private Font font;
	private HBBuffer buffer;
	private Feature[] features;
	private ListWrapper wrappedFeatures;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var fontBytes = LoadFont();
		blob = Blob.FromStream(new System.IO.MemoryStream(fontBytes));
		face = new Face(blob, 0)
		{
			UnitsPerEm = 2048,
		};
		font = new Font(face);
		font.SetScale(2048, 2048);
		font.SetFunctionsOpenType();

		Tag[] tags = { new Tag('l', 'i', 'g', 'a'), new Tag('k', 'e', 'r', 'n'),
			new Tag('s', 'm', 'c', 'p'), new Tag('t', 'n', 'u', 'm'),
			new Tag('c', 'a', 'l', 't'), new Tag('d', 'l', 'i', 'g'),
			new Tag('o', 'n', 'u', 'm'), new Tag('s', 's', '0', '1') };
		features = new Feature[FeatureCount];
		for (var i = 0; i < FeatureCount; i++)
			features[i] = new Feature(tags[i % tags.Length], 1);
		wrappedFeatures = new ListWrapper(features);

		buffer = new HBBuffer();
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		buffer.Dispose();
		font.Dispose();
		face.Dispose();
		blob.Dispose();
	}

	// New: Feature[] handed in directly — the shipped Shape pins it with no ToArray() copy.
	[Benchmark]
	public int New()
	{
		buffer.ClearContents();
		buffer.AddUtf8("The quick brown fox.");
		buffer.GuessSegmentProperties();
		font.Shape(buffer, features, null);
		return buffer.Length;
	}

	// Old: a non-array IReadOnlyList<Feature> — forces the ToArray() fallback, matching the previous
	// unconditional copy on every call.
	[Benchmark(Baseline = true)]
	public int Old()
	{
		buffer.ClearContents();
		buffer.AddUtf8("The quick brown fox.");
		buffer.GuessSegmentProperties();
		font.Shape(buffer, wrappedFeatures, null);
		return buffer.Length;
	}

	private static byte[] LoadFont()
	{
		var assembly = typeof(HarfBuzzShapeFeaturesBenchmark).Assembly;
		using var stream = assembly.GetManifestResourceStream("content-font.ttf");
		using var ms = new System.IO.MemoryStream();
		stream.CopyTo(ms);
		return ms.ToArray();
	}

	// A minimal non-array IReadOnlyList<Feature> so Font.Shape's `features as Feature[]` misses and
	// the ToArray() fallback runs — reproducing the previous per-call allocation.
	private sealed class ListWrapper : IReadOnlyList<Feature>
	{
		private readonly Feature[] items;
		public ListWrapper(Feature[] items) => this.items = items;
		public Feature this[int index] => items[index];
		public int Count => items.Length;
		public IEnumerator<Feature> GetEnumerator() => ((IEnumerable<Feature>)items).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
	}
}
