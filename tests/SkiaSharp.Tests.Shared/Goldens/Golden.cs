using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests.Shared;

/// <summary>A resolved golden case that verifies file-backed, encoded, or raw pixel captures.</summary>
public sealed class Golden
{
	private const string MarkerPrefix = "##SKIA-GOLDEN##";
	private const double DefaultMaxErrorPixelFraction = 0.00075;

	private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

	private readonly Func<byte[]>? readReference;

	internal Golden(
		GoldenCase goldenCase,
		string? referenceKey,
		string? referenceLocation,
		IEnumerable<string> attempts,
		Func<byte[]>? readReference)
	{
		Case = goldenCase ?? throw new ArgumentNullException(nameof(goldenCase));
		ReferenceKey = referenceKey;
		ReferenceLocation = referenceLocation;
		Attempts = attempts.ToList().AsReadOnly();
		this.readReference = readReference;
	}

	public GoldenCase Case { get; }
	public string CaptureKey => Case.CaptureKey;
	public string? ReferenceKey { get; }
	public string? ReferenceLocation { get; }
	public IReadOnlyList<string> Attempts { get; }

	public byte[] ReadBytes() =>
		readReference == null ? throw new InvalidOperationException(Describe()) : readReference();

	public string Describe() =>
		"Golden request: " + Case.Profile +
		(Case.Version == null ? "" : " at " + Case.Version) +
		"/" + Case.CasePath + Environment.NewLine +
		(ReferenceKey == null
			? "MISSING reference"
			: "Selected: " + ReferenceKey + " at " + ReferenceLocation) +
		Environment.NewLine + "Attempted (in order):" + Environment.NewLine + "  " +
		string.Join(Environment.NewLine + "  ", Attempts);

	public string FormatMarker(string role, byte[] png)
	{
		if (role != "actual" && role != "reference" && role != "diff")
			throw new ArgumentException("Unknown image role.", nameof(role));
		if (png == null) throw new ArgumentNullException(nameof(png));
		using var image = Decode(png, role + " marker");
		return Marker(role, png, new SKSizeI(image.Width, image.Height));
	}

	public string Verify(
		string actualPath,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (actualPath == null) throw new ArgumentNullException(nameof(actualPath));
		VerifyReference(
			File.ReadAllBytes(actualPath), actualPath, actualPath,
			expectedSize, channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		return actualPath;
	}

	public void Verify(
		byte[] actualPng,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (actualPng == null) throw new ArgumentNullException(nameof(actualPng));
		VerifyReference(
			actualPng, Path.GetFileName(CaptureKey), null,
			expectedSize, channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
	}

	public void Verify(
		byte[] actualPixels,
		SKImageInfo imageInfo,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null) =>
		Verify(
			EncodePng(actualPixels, imageInfo),
			expectedSize,
			channelTolerance,
			maxErrorPixelFraction,
			maxAverageColorErrorFraction);

	public Task<string> VerifyAsync(
		Func<Task<string>> capturePath,
		TimeSpan timeout,
		CancellationToken cancellationToken = default,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (capturePath == null) throw new ArgumentNullException(nameof(capturePath));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
		ValidateComparison(channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		var expectedPng = ReferenceKey == null ? null : ReadBytes();
		return RetryAsync(
			capturePath,
			timeout,
			cancellationToken,
			(actualPng, actualPath, report) => VerifyCore(
				this,
				actualPng, actualPath, actualPath,
				expectedPng, ReferenceLocation,
				ReferenceLocation != null && File.Exists(ReferenceLocation) ? ReferenceLocation : null,
				expectedSize, channelTolerance, maxErrorPixelFraction,
				maxAverageColorErrorFraction, report));
	}

	public static string Verify(
		string actualPath,
		string expectedPath,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (actualPath == null) throw new ArgumentNullException(nameof(actualPath));
		if (expectedPath == null) throw new ArgumentNullException(nameof(expectedPath));
		VerifyDirect(
			File.ReadAllBytes(actualPath), actualPath, actualPath,
			File.ReadAllBytes(expectedPath), expectedPath, expectedPath,
			expectedSize, channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		return actualPath;
	}

	public static void Verify(
		byte[] actualPng,
		byte[] expectedPng,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (actualPng == null) throw new ArgumentNullException(nameof(actualPng));
		if (expectedPng == null) throw new ArgumentNullException(nameof(expectedPng));
		VerifyDirect(
			actualPng, "actual.png", null,
			expectedPng, "expected.png", null,
			expectedSize, channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
	}

	public static Task<string> VerifyAsync(
		Func<Task<string>> capturePath,
		string expectedPath,
		TimeSpan timeout,
		CancellationToken cancellationToken = default,
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null)
	{
		if (expectedPath == null) throw new ArgumentNullException(nameof(expectedPath));
		ValidateComparison(channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		byte[]? expectedPng = null;
		return RetryAsync(
			capturePath,
			timeout,
			cancellationToken,
			(actualPng, actualPath, report) => VerifyCore(
				null,
				actualPng, actualPath, actualPath,
				expectedPng ??= File.ReadAllBytes(expectedPath), expectedPath, expectedPath,
				expectedSize, channelTolerance, maxErrorPixelFraction,
				maxAverageColorErrorFraction, report));
	}

	public static string WritePng(string path, byte[] png)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (png == null) throw new ArgumentNullException(nameof(png));
		if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
			Directory.CreateDirectory(directory);
		File.WriteAllBytes(path, png);
		if (!HasPngSignature(png))
			throw new InvalidOperationException("Expected a PNG image.");
		return path;
	}

	private static byte[] EncodePng(SKImage image)
	{
		if (image == null) throw new ArgumentNullException(nameof(image));
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		return data.ToArray();
	}

	private static byte[] EncodePng(byte[] pixels, SKImageInfo imageInfo)
	{
		if (pixels == null) throw new ArgumentNullException(nameof(pixels));
		var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
		try
		{
			using var pixmap = new SKPixmap(imageInfo, handle.AddrOfPinnedObject());
			using var image = SKImage.FromPixels(pixmap);
			return EncodePng(image);
		}
		finally
		{
			handle.Free();
		}
	}

	private void VerifyReference(
		byte[] actualPng,
		string actualLabel,
		string? actualPath,
		SKSizeI? expectedSize,
		int channelTolerance,
		double maxErrorPixelFraction,
		double? maxAverageColorErrorFraction)
	{
		ValidateComparison(channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		var expectedPng = ReferenceKey == null ? null : ReadBytes();
		VerifyCore(
			this,
			actualPng, actualLabel, actualPath,
			expectedPng, ReferenceLocation,
			ReferenceLocation != null && File.Exists(ReferenceLocation) ? ReferenceLocation : null,
			expectedSize, channelTolerance, maxErrorPixelFraction,
			maxAverageColorErrorFraction, report: true);
	}

	private static bool VerifyCore(
		Golden? golden,
		byte[] actualPng,
		string actualLabel,
		string? actualPath,
		byte[]? expectedPng,
		string? expectedLabel,
		string? expectedPath,
		SKSizeI? expectedSize,
		int channelTolerance,
		double maxErrorPixelFraction,
		double? maxAverageColorErrorFraction,
		bool report)
	{
		if (expectedPng == null)
		{
			if (golden == null)
				throw new InvalidOperationException("Direct image verification requires an expected image.");
			using var actualImage = Decode(actualPng, actualLabel);
			if (expectedSize is { } required &&
				(actualImage.Width != required.Width || actualImage.Height != required.Height))
			{
				if (!report)
					return false;
				Attach(actualPng, actualLabel, actualPath);
				Assert.Fail("Image is " + Size(actualImage) + ", expected " + Size(required) + ".");
			}

			golden.WriteDiagnostics(
				actualPng,
				new SKSizeI(actualImage.Width, actualImage.Height),
				null,
				null,
				null);
			Attach(actualPng, actualLabel, actualPath);
			Assert.Fail(
				"Missing reviewed golden. " + golden.Describe() +
				$" Actual retained at {actualLabel}.");
			return false;
		}

		var matched = Compare(
			actualPng, actualLabel, expectedPng, expectedLabel!,
			expectedSize, channelTolerance, maxErrorPixelFraction,
			maxAverageColorErrorFraction, report,
			out var actualImageSize, out var expectedImageSize,
			out var comparison, out var differencePng, out var failure);
		if (!matched && !report)
			return false;

		golden?.WriteDiagnostics(
			actualPng, actualImageSize, expectedPng, expectedImageSize, differencePng);
		Complete(
			actualPng, actualLabel, actualPath,
			expectedPng, expectedLabel!, expectedPath,
			comparison, differencePng, failure, matched);
		return matched;
	}

	private static void VerifyDirect(
		byte[] actualPng,
		string actualLabel,
		string? actualPath,
		byte[] expectedPng,
		string expectedLabel,
		string? expectedPath,
		SKSizeI? expectedSize,
		int channelTolerance,
		double maxErrorPixelFraction,
		double? maxAverageColorErrorFraction)
	{
		ValidateComparison(channelTolerance, maxErrorPixelFraction, maxAverageColorErrorFraction);
		VerifyCore(
			null,
			actualPng, actualLabel, actualPath,
			expectedPng, expectedLabel, expectedPath,
			expectedSize, channelTolerance, maxErrorPixelFraction,
			maxAverageColorErrorFraction, report: true);
	}

	private static bool Compare(
		byte[] actualPng,
		string actualLabel,
		byte[] expectedPng,
		string expectedLabel,
		SKSizeI? expectedSize,
		int channelTolerance,
		double maxErrorPixelFraction,
		double? maxAverageColorErrorFraction,
		bool createDifference,
		out SKSizeI actualImageSize,
		out SKSizeI expectedImageSize,
		out SKPixelComparisonResult? comparison,
		out byte[]? differencePng,
		out string? failure)
	{
		using var actualImage = Decode(actualPng, actualLabel);
		using var expectedImage = Decode(expectedPng, expectedLabel);
		actualImageSize = new SKSizeI(actualImage.Width, actualImage.Height);
		expectedImageSize = new SKSizeI(expectedImage.Width, expectedImage.Height);
		comparison = null;
		differencePng = null;

		if (actualImageSize != expectedImageSize)
		{
			failure = "Image dimensions differ: actual " + Size(actualImageSize) +
				", expected " + Size(expectedImageSize) + ".";
			return false;
		}
		if (expectedSize is { } required && actualImageSize != required)
		{
			failure = "Image is " + Size(actualImageSize) + ", expected " + Size(required) + ".";
			return false;
		}

		comparison = SKPixelComparer.Compare(expectedImage, actualImage, channelTolerance);
		var matched = maxAverageColorErrorFraction is { } average
			? comparison.AverageColorErrorFraction <= average
			: comparison.ErrorPixelCount <=
				(long)Math.Floor(comparison.TotalPixels * maxErrorPixelFraction);
		if (matched)
		{
			failure = null;
			return true;
		}

		failure = maxAverageColorErrorFraction is { } limit
			? $"average RGBA color error {comparison.AverageColorErrorFraction:P3} exceeds {limit:P3}."
			: $"{comparison.ErrorPixelCount}/{comparison.TotalPixels} pixels differ, exceeding " +
				$"{maxErrorPixelFraction:P3}; max channel delta {comparison.MaxChannelDelta}.";
		if (createDifference)
		{
			using var difference = SKPixelComparer.GenerateDifferenceImage(
				expectedImage, actualImage, channelTolerance);
			differencePng = EncodePng(difference);
		}
		return false;
	}

	private void WriteDiagnostics(
		byte[] actualPng,
		SKSizeI actualSize,
		byte[]? expectedPng,
		SKSizeI? expectedSize,
		byte[]? differencePng)
	{
		TestContext.Current.TestOutputHelper?.WriteLine(Describe());
		TestContext.Current.TestOutputHelper?.WriteLine(
			Marker("actual", actualPng, actualSize));
		if (expectedPng != null && expectedSize is { } referenceSize)
			TestContext.Current.TestOutputHelper?.WriteLine(
				Marker("reference", expectedPng, referenceSize));
		if (differencePng != null)
			TestContext.Current.TestOutputHelper?.WriteLine(
				Marker("diff", differencePng, actualSize));
	}

	private static void Complete(
		byte[] actualPng,
		string actualLabel,
		string? actualPath,
		byte[] expectedPng,
		string expectedLabel,
		string? expectedPath,
		SKPixelComparisonResult? comparison,
		byte[]? differencePng,
		string? failure,
		bool matched)
	{
		Attach(actualPng, actualLabel, actualPath);
		if (comparison != null)
		{
			TestContext.Current.TestOutputHelper?.WriteLine(
				$"Image comparison: {comparison.ErrorPixelCount}/{comparison.TotalPixels} pixels differ; " +
				$"average RGBA color error {comparison.AverageColorErrorFraction:P3}.");
		}
		if (matched)
			return;

		Attach(
			expectedPng,
			"expected-" + Path.GetFileName(expectedLabel),
			expectedPath,
			"expected-" + Path.GetFileName(expectedLabel));
		string? diffPath = null;
		if (differencePng != null)
		{
			if (actualPath != null)
			{
				diffPath = Path.ChangeExtension(actualPath, ".diff.png");
				WritePng(diffPath, differencePng);
				TestContext.Current.AddFileAttachment(diffPath, "image/png");
			}
			else
			{
				TestArtifacts.AddAttachment(
					TestContext.Current, "difference.png", differencePng, "image/png");
			}
		}

		Assert.Fail(
			$"Golden images differ: {failure} Expected {expectedLabel}; actual {actualLabel}" +
			(diffPath == null ? "." : $"; diff {diffPath}."));
	}

	private static void Attach(
		byte[] png,
		string label,
		string? path,
		string? attachmentName = null)
	{
		if (path != null)
			TestContext.Current.AddFileAttachment(
				path, "image/png", attachmentName ?? Path.GetFileName(path));
		else
			TestArtifacts.AddAttachment(
				TestContext.Current,
				attachmentName ?? Path.GetFileName(label),
				png,
				"image/png");
	}

	private string Marker(string role, byte[] png, SKSizeI size) =>
		MarkerPrefix + " role=" + role + " key=" + CaptureKey +
		" size=" + size.Width + "x" + size.Height +
		" base64=" + Convert.ToBase64String(png);

	private static SKImage Decode(byte[] png, string location) =>
		SKImage.FromEncodedData(png)
		?? throw new InvalidOperationException("Failed to decode image: " + location + ".");

	private static bool HasPngSignature(byte[] bytes)
	{
		if (bytes.Length < PngSignature.Length)
			return false;
		for (var i = 0; i < PngSignature.Length; i++)
			if (bytes[i] != PngSignature[i])
				return false;
		return true;
	}

	private static void ValidateComparison(
		int channelTolerance,
		double maxErrorPixelFraction,
		double? maxAverageColorErrorFraction)
	{
		if (channelTolerance < 0 || channelTolerance > byte.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(channelTolerance));
		if (double.IsNaN(maxErrorPixelFraction) ||
			maxErrorPixelFraction < 0 || maxErrorPixelFraction > 1)
			throw new ArgumentOutOfRangeException(nameof(maxErrorPixelFraction));
		if (maxAverageColorErrorFraction is { } average &&
			(double.IsNaN(average) || average < 0 || average > 1))
			throw new ArgumentOutOfRangeException(nameof(maxAverageColorErrorFraction));
	}

	private static async Task<string> RetryAsync(
		Func<Task<string>> capturePath,
		TimeSpan timeout,
		CancellationToken cancellationToken,
		Func<byte[], string, bool, bool> verify)
	{
		if (capturePath == null) throw new ArgumentNullException(nameof(capturePath));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

		var clock = Stopwatch.StartNew();
		string? actualPath = null;
		byte[]? actualPng = null;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var remaining = timeout - clock.Elapsed;
			if (remaining <= TimeSpan.Zero)
			{
				if (actualPng == null || actualPath == null)
					throw new TimeoutException("The image capture deadline expired before capture completed.");
				verify(actualPng, actualPath, true);
				return actualPath;
			}

			actualPath = await WaitAsync(capturePath(), remaining, cancellationToken);
			actualPng = File.ReadAllBytes(actualPath);
			if (verify(actualPng, actualPath, false))
				return actualPath;

			remaining = timeout - clock.Elapsed;
			if (remaining > TimeSpan.Zero)
				await Task.Delay(
					remaining < TimeSpan.FromMilliseconds(100)
						? remaining
						: TimeSpan.FromMilliseconds(100),
					cancellationToken);
		}
	}

	private static async Task<T> WaitAsync<T>(
		Task<T> task,
		TimeSpan timeout,
		CancellationToken cancellationToken)
	{
		using var timeoutCancellation =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var delay = Task.Delay(timeout, timeoutCancellation.Token);
		if (await Task.WhenAny(task, delay) != task)
		{
			cancellationToken.ThrowIfCancellationRequested();
			throw new TimeoutException("The image capture deadline expired.");
		}

		timeoutCancellation.Cancel();
		return await task;
	}

	private static string Size(SKImage image) =>
		image.Width + "x" + image.Height;

	private static string Size(SKSizeI size) =>
		size.Width + "x" + size.Height;
}
