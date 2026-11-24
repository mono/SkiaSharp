using System;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using HarfBuzzSharp;
using SkiaSharp.HarfBuzz;
using Buffer = HarfBuzzSharp.Buffer;

namespace SkiaSharp.Benchmarks;

// SKShaper.Shape(...) is the per-shaped-run text path behind SKCanvas.DrawShapedText. After native
// HarfBuzz shaping it extracts the results into the SKShaper.Result arrays. The shipped code reads
// the shaping output through buffer.GlyphInfos and buffer.GlyphPositions — and each of those getters
// does a .ToArray() copy of the native glyph buffer (Buffer.cs: GetGlyphInfoSpan().ToArray()) — then
// iterates each array exactly once. Those two copies are pure per-shape waste: the zero-copy
// buffer.GetGlyphInfoSpan()/GetGlyphPositionSpan() expose the very same data with no allocation.
//
// This benchmark isolates the result-extraction step (the only part the fix changes) against an
// already-shaped buffer, so native shaping is held constant and the delta is exactly the two removed
// glyph arrays. Old = the shipped array-getter extraction copied verbatim; New = the same loop over
// the zero-copy spans. Both still allocate the three real Result arrays (points/clusters/codepoints),
// because the fix does not — and must not — remove those.
[Config(typeof(Config))]
[MemoryDiagnoser]
public class SKShaperGlyphExtractionBenchmark
{
	// Runs in-process so BenchmarkDotNet does not spawn a generated project that rebuilds the
	// multi-targeted binding projects (their mobile TFMs need SDK workloads that are not installed
	// on this desktop/CI host). MemoryDiagnoser and the New-vs-Old timing are fully supported
	// in-process; this only changes where the benchmark executes, not what is measured.
	private class Config : ManualConfig
	{
		public Config() =>
			AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
	}

	// Mirrors SKShaper.FONT_SIZE_SCALE (internal to SkiaSharp.HarfBuzz).
	private const int FontSizeScale = 512;

	// Approximate shaped-glyph counts: a word, a short line, a paragraph-ish run.
	[Params(3, 24, 96)]
	public int Glyphs { get; set; }

	private SKTypeface typeface;
	private Blob blob;
	private Face face;
	private Font hbFont;
	private Buffer buffer;

	private int len;
	private float textSizeX;
	private float textSizeY;

	[GlobalSetup]
	public void Setup()
	{
		var fontBytes = LoadFont();
		using (var data = SKData.CreateCopy(fontBytes))
			typeface = SKTypeface.FromData(data);

		// Build the HarfBuzz font exactly as SKShaper's constructor does.
		blob = typeface.OpenStream(out var index).ToHarfBuzzBlob();
		face = new Face(blob, index)
		{
			Index = index,
			UnitsPerEm = typeface.UnitsPerEm,
		};
		hbFont = new Font(face);
		hbFont.SetScale(FontSizeScale, FontSizeScale);
		hbFont.SetFunctionsOpenType();

		// A realistic complex-script (Arabic) run of roughly `Glyphs` glyphs.
		var reps = Math.Max(1, Glyphs / 3);
		var sb = new StringBuilder(reps * 4);
		for (var i = 0; i < reps; i++)
		{
			if (i > 0)
				sb.Append(' ');
			sb.Append("متن");
		}

		buffer = new Buffer();
		buffer.AddUtf8(sb.ToString());
		buffer.GuessSegmentProperties();
		hbFont.Shape(buffer);

		len = buffer.Length;

		// SKShaper computes these from font.Size / font.ScaleX (default ScaleX == 1).
		const float fontSize = 64f;
		textSizeY = fontSize / FontSizeScale;
		textSizeX = textSizeY * 1f;
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		buffer?.Dispose();
		hbFont?.Dispose();
		face?.Dispose();
		blob?.Dispose();
		typeface?.Dispose();
	}

	// Old: the shipped SKShaper.Shape extraction — buffer.GlyphInfos/GlyphPositions each allocate a
	// GlyphInfo[]/GlyphPosition[] copy (via ToArray) that is read once and discarded.
	[Benchmark(Baseline = true)]
	public int Old()
	{
		var info = buffer.GlyphInfos;
		var pos = buffer.GlyphPositions;

		var points = new SKPoint[len];
		var clusters = new uint[len];
		var codepoints = new uint[len];

		float xOffset = 0, yOffset = 0;
		for (var i = 0; i < len; i++)
		{
			codepoints[i] = info[i].Codepoint;
			clusters[i] = info[i].Cluster;
			points[i] = new SKPoint(
				xOffset + pos[i].XOffset * textSizeX,
				yOffset - pos[i].YOffset * textSizeY);
			xOffset += pos[i].XAdvance * textSizeX;
			yOffset += pos[i].YAdvance * textSizeY;
		}

		return points.Length + clusters.Length + codepoints.Length;
	}

	// New: identical extraction over the zero-copy spans — no GlyphInfo[]/GlyphPosition[] copy.
	[Benchmark]
	public int New()
	{
		var info = buffer.GetGlyphInfoSpan();
		var pos = buffer.GetGlyphPositionSpan();

		var points = new SKPoint[len];
		var clusters = new uint[len];
		var codepoints = new uint[len];

		float xOffset = 0, yOffset = 0;
		for (var i = 0; i < len; i++)
		{
			codepoints[i] = info[i].Codepoint;
			clusters[i] = info[i].Cluster;
			points[i] = new SKPoint(
				xOffset + pos[i].XOffset * textSizeX,
				yOffset - pos[i].YOffset * textSizeY);
			xOffset += pos[i].XAdvance * textSizeX;
			yOffset += pos[i].YAdvance * textSizeY;
		}

		return points.Length + clusters.Length + codepoints.Length;
	}

	private static byte[] LoadFont()
	{
		var asm = typeof(SKShaperGlyphExtractionBenchmark).Assembly;
		using var stream = asm.GetManifestResourceStream("content-font.ttf")
			?? throw new InvalidOperationException("Embedded font 'content-font.ttf' was not found.");
		var bytes = new byte[stream.Length];
		var read = 0;
		while (read < bytes.Length)
		{
			var n = stream.Read(bytes, read, bytes.Length - read);
			if (n == 0)
				break;
			read += n;
		}
		return bytes;
	}
}
