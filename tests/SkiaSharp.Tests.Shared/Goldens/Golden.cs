using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Tests.Shared;

/// <summary>A resolved golden case that verifies file-backed, encoded, or raw pixel captures.</summary>
public sealed class Golden
{
	private const string MarkerPrefix = "##SKIA-GOLDEN##";

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

	public string Describe()
	{
		var version = Case.Version == null ? "" : $" at {Case.Version}";
		var selected = ReferenceKey == null
			? "MISSING reference"
			: $"Selected: {ReferenceKey} at {ReferenceLocation}";
		var indent = $"{Environment.NewLine}  ";
		return string.Join(
			Environment.NewLine,
			$"Golden request: {Case.Profile}{version}/{Case.CasePath}",
			selected,
			$"Attempted (in order):{indent}{string.Join(indent, Attempts)}");
	}

	public string FormatMarker(string role, byte[] png)
	{
		if (role != "actual" && role != "reference" && role != "diff")
			throw new ArgumentException("Unknown image role.", nameof(role));
		if (png == null)
			throw new ArgumentNullException(nameof(png));

		using var image = Decode(png, $"{role} marker");
		return Marker(role, png, image.Info.Size);
	}

	public string Verify(GoldenInput actual, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));

		options = GoldenOptions.OrDefault(options);
		var image = actual.Read();
		VerifyCore(this, image, ReferenceInput(), options);
		return image.Label;
	}

	public Task<string> VerifyAsync(GoldenInput actual, TimeSpan timeout,
		CancellationToken cancellationToken = default, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

		options = GoldenOptions.OrDefault(options);
		var expected = ReferenceInput();
		return RetryAsync(actual, timeout, cancellationToken,
			(image, report) => VerifyCore(this, image, expected, options, report));
	}

	public static string Verify(GoldenInput actual, GoldenInput expected, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));

		options = GoldenOptions.OrDefault(options);
		var image = actual.Read();
		VerifyCore(null, image, expected.Read(), options);
		return image.Label;
	}

	public static Task<string> VerifyAsync(GoldenInput actual, GoldenInput expected, TimeSpan timeout,
		CancellationToken cancellationToken = default, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));

		options = GoldenOptions.OrDefault(options);
		GoldenInput.Image? expectedImage = null;
		return RetryAsync(actual, timeout, cancellationToken,
			(image, report) => VerifyCore(null, image, expectedImage ??= expected.Read(), options, report));
	}

	private static void WritePng(string path, byte[] png)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (png == null) throw new ArgumentNullException(nameof(png));

		if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
			Directory.CreateDirectory(directory);
		File.WriteAllBytes(path, png);
		if (!GoldenInput.HasPngSignature(png))
			throw new InvalidOperationException("Expected a PNG image.");
	}

	private GoldenInput.Image? ReferenceInput()
	{
		if (ReferenceKey == null)
			return null;

		return ReferenceLocation != null && File.Exists(ReferenceLocation)
			? GoldenInput.Path(ReferenceLocation).Read()
			: GoldenInput.Bytes(ReadBytes(), ReferenceKey).Read();
	}

	private static bool VerifyCore(
		Golden? golden, GoldenInput.Image actual, GoldenInput.Image? expected, GoldenOptions options, bool report = true)
	{
		if (expected == null)
		{
			if (golden == null)
				throw new InvalidOperationException("Direct image verification requires an expected image.");

			using var actualImage = Decode(actual.Png, actual.Label);
			var actualSize = actualImage.Info.Size;
			if (options.ExpectedSize is { } required && actualSize != required)
			{
				if (!report)
					return false;
				Attach(actual);
				Assert.Fail($"Image is {Size(actualSize)}, expected {Size(required)}.");
			}

			golden.WriteDiagnostics(actual, actualSize);
			Attach(actual);
			Assert.Fail($"Missing reviewed golden. {golden.Describe()} Actual retained at {actual.Label}.");
			return false;
		}

		var result = Compare(actual.Png, actual.Label, expected.Png, expected.Label, options, report);
		if (!result.Matched && !report)
			return false;

		golden?.WriteDiagnostics(actual, expected, result);

		Complete(actual, expected, result);

		return result.Matched;
	}

	private static Comparison Compare(byte[] actualPng, string actualLabel,
		byte[] expectedPng, string expectedLabel, GoldenOptions options, bool createDifference)
	{
		using var actualImage = Decode(actualPng, actualLabel);
		using var expectedImage = Decode(expectedPng, expectedLabel);
		var actualSize = actualImage.Info.Size;
		var expectedSize = expectedImage.Info.Size;

		if (actualSize != expectedSize)
		{
			var failure = $"Image dimensions differ: actual {Size(actualSize)}, expected {Size(expectedSize)}.";
			return new(false, actualSize, expectedSize, null, null, failure);
		}
		if (options.ExpectedSize is { } required && actualSize != required)
		{
			var failure = $"Image is {Size(actualSize)}, expected {Size(required)}.";
			return new(false, actualSize, expectedSize, null, null, failure);
		}

		var pixels = SKPixelComparer.Compare(expectedImage, actualImage, options.ChannelTolerance);
		var matched = options.MaxAverageColorErrorFraction is { } average
			? pixels.AverageColorErrorFraction <= average
			: pixels.ErrorPixelCount <= (long)Math.Floor(pixels.TotalPixels * options.MaxErrorPixelFraction);
		if (matched)
			return new(true, actualSize, expectedSize, pixels, null, null);

		var pixelDifference = $"{pixels.ErrorPixelCount}/{pixels.TotalPixels} pixels differ";
		var budget = $"{pixelDifference}, exceeding {options.MaxErrorPixelFraction:P3};";
		var message = options.MaxAverageColorErrorFraction is { } limit
			? $"average RGBA color error {pixels.AverageColorErrorFraction:P3} exceeds {limit:P3}."
			: $"{budget} max channel delta {pixels.MaxChannelDelta}.";
		byte[]? differencePng = null;
		if (createDifference)
		{
			using var difference = SKPixelComparer.GenerateDifferenceImage(
				expectedImage, actualImage, options.ChannelTolerance);
			using var data = difference.Encode(SKEncodedImageFormat.Png, 100);
			differencePng = data.ToArray();
		}
		return new(false, actualSize, expectedSize, pixels, differencePng, message);
	}

	private void WriteDiagnostics(GoldenInput.Image actual, SKSizeI actualSize)
	{
		TestContext.Current.TestOutputHelper?.WriteLine(Describe());
		TestContext.Current.TestOutputHelper?.WriteLine(Marker("actual", actual.Png, actualSize));
	}

	private void WriteDiagnostics(GoldenInput.Image actual, GoldenInput.Image expected, Comparison comparison)
	{
		TestContext.Current.TestOutputHelper?.WriteLine(Describe());
		TestContext.Current.TestOutputHelper?.WriteLine(Marker("actual", actual.Png, comparison.ActualSize));
		TestContext.Current.TestOutputHelper?.WriteLine(Marker("reference", expected.Png, comparison.ExpectedSize));
		if (comparison.DifferencePng != null)
			TestContext.Current.TestOutputHelper?.WriteLine(Marker("diff", comparison.DifferencePng, comparison.ActualSize));
	}

	private static void Complete(GoldenInput.Image actual, GoldenInput.Image expected, Comparison comparison)
	{
		Attach(actual);
		if (comparison.Pixels != null)
		{
			var difference =
				$"{comparison.Pixels.ErrorPixelCount}/{comparison.Pixels.TotalPixels} pixels differ";
			var summary = $"Image comparison: {difference};";
			TestContext.Current.TestOutputHelper?.WriteLine(
				$"{summary} average RGBA color error {comparison.Pixels.AverageColorErrorFraction:P3}.");
		}
		if (comparison.Matched)
			return;

		Attach(expected, $"expected-{expected.AttachmentName}");
		string? diffPath = null;
		if (comparison.DifferencePng != null)
		{
			if (actual.FilePath != null)
			{
				diffPath = Path.ChangeExtension(actual.FilePath, ".diff.png");
				WritePng(diffPath, comparison.DifferencePng);
				TestContext.Current.AddFileAttachment(diffPath, "image/png");
			}
			else
			{
				TestArtifacts.AddAttachment(TestContext.Current, "difference.png", comparison.DifferencePng, "image/png");
			}
		}

		var diff = diffPath == null ? "." : $"; diff {diffPath}.";
		var locations = $"Expected {expected.Label}; actual {actual.Label}{diff}";
		Assert.Fail($"Golden images differ: {comparison.Failure} {locations}");
	}

	private static void Attach(GoldenInput.Image image, string? attachmentName = null)
	{
		if (image.FilePath != null)
			TestContext.Current.AddFileAttachment(
				image.FilePath,
				"image/png",
				attachmentName ?? image.AttachmentName);
		else
			TestArtifacts.AddAttachment(
				TestContext.Current,
				attachmentName ?? image.AttachmentName,
				image.Png,
				"image/png");
	}

	private string Marker(string role, byte[] png, SKSizeI size)
	{
		var base64 = Convert.ToBase64String(png);
		var header = $"{MarkerPrefix} role={role} key={CaptureKey}";
		return $"{header} size={size.Width}x{size.Height} base64={base64}";
	}

	private static SKImage Decode(byte[] png, string location) =>
		SKImage.FromEncodedData(png) ?? throw new InvalidOperationException($"Failed to decode image: {location}.");

	private static async Task<string> RetryAsync(
		GoldenInput input,
		TimeSpan timeout,
		CancellationToken cancellationToken,
		Func<GoldenInput.Image, bool, bool> verify)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

		var clock = Stopwatch.StartNew();
		GoldenInput.Image? actual = null;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var remaining = timeout - clock.Elapsed;
			if (remaining <= TimeSpan.Zero)
			{
				if (actual == null)
					throw new TimeoutException("The image capture deadline expired before capture completed.");
				verify(actual, true);
				return actual.Label;
			}

			actual = await WaitAsync(input.ReadAsync(), remaining, cancellationToken);
			if (verify(actual, false))
				return actual.Label;

			remaining = timeout - clock.Elapsed;
			if (remaining > TimeSpan.Zero)
				await Task.Delay(
					remaining < TimeSpan.FromMilliseconds(100)
						? remaining
						: TimeSpan.FromMilliseconds(100),
					cancellationToken);
		}
	}

	private static async Task<T> WaitAsync<T>(Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
	{
		using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var delay = Task.Delay(timeout, timeoutCancellation.Token);
		if (await Task.WhenAny(task, delay) != task)
		{
			cancellationToken.ThrowIfCancellationRequested();
			throw new TimeoutException("The image capture deadline expired.");
		}

		timeoutCancellation.Cancel();
		return await task;
	}

	private static string Size(SKSizeI size) =>
		$"{size.Width}x{size.Height}";

	private sealed record Comparison(
		bool Matched,
		SKSizeI ActualSize,
		SKSizeI ExpectedSize,
		SKPixelComparisonResult? Pixels,
		byte[]? DifferencePng,
		string? Failure);
}
