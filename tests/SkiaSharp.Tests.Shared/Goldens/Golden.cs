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
		IEnumerable<string> candidateKeys,
		Func<byte[]>? readReference)
	{
		Case = goldenCase ?? throw new ArgumentNullException(nameof(goldenCase));
		ReferenceKey = referenceKey;
		ReferenceLocation = referenceLocation;
		CandidateKeys = candidateKeys.ToList().AsReadOnly();
		this.readReference = readReference;
	}

	/// <summary>The requested runtime and case metadata used to resolve this golden.</summary>
	public GoldenCase Case { get; }

	/// <summary>The primary candidate key for capturing or proposing a reference image.</summary>
	public string CaptureKey => Case.CaptureKey;

	/// <summary>The matched reference key in the store, or <see langword="null"/> if no reference was found.</summary>
	public string? ReferenceKey { get; }

	/// <summary>The file path or embedded resource location of the matched reference, or <see langword="null"/> if missing.</summary>
	public string? ReferenceLocation { get; }

	/// <summary>The ordered candidate keys checked across profile, version, and architecture fallbacks during resolution.</summary>
	public IReadOnlyList<string> CandidateKeys { get; }

	/// <summary>Reads the encoded PNG bytes of the resolved reference image.</summary>
	/// <returns>The encoded PNG bytes of the matched reference image.</returns>
	/// <exception cref="InvalidOperationException">No reference image was resolved for <see cref="Case"/>.</exception>
	public byte[] ReadBytes() =>
		readReference == null ? throw new InvalidOperationException(Describe()) : readReference();

	/// <summary>Formats a diagnostic summary of the requested case, selected reference, and fallback keys checked.</summary>
	/// <returns>A multi-line summary of the golden lookup and all <see cref="CandidateKeys"/>.</returns>
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
			$"Attempted (in order):{indent}{string.Join(indent, CandidateKeys)}");
	}

	/// <summary>Verifies <paramref name="actual"/> against this golden's resolved reference image once.</summary>
	/// <param name="actual">The captured image path, encoded bytes, or raw pixels to verify.</param>
	/// <param name="options">Optional dimension and pixel tolerance overrides; <see langword="null"/> uses <see cref="GoldenOptions"/> defaults.</param>
	/// <returns>The label or file path of the verified <paramref name="actual"/> image.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="actual"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException"><paramref name="actual"/> is an asynchronous capture input or cannot be decoded.</exception>
	public string Verify(GoldenInput actual, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));

		options = GoldenOptions.OrDefault(options);
		var image = actual.Read();
		VerifyCore(this, image, ReferenceInput(), options);
		return image.Label;
	}

	/// <summary>Repeatedly captures and verifies <paramref name="actual"/> against this golden's resolved reference until it matches or <paramref name="timeout"/> expires.</summary>
	/// <param name="actual">The image input or asynchronous capture callback to invoke on each attempt.</param>
	/// <param name="timeout">The total time budget for capturing a matching image.</param>
	/// <param name="cancellationToken">A token to cancel waiting or image capture.</param>
	/// <param name="options">Optional dimension and pixel tolerance overrides; <see langword="null"/> uses <see cref="GoldenOptions"/> defaults.</param>
	/// <param name="retryInterval">The delay between retry captures after the first confirmation retry; <see langword="null"/> defaults to 100 ms.</param>
	/// <returns>A task that resolves to the label or file path of the verified <paramref name="actual"/> image.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="actual"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> or <paramref name="retryInterval"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</exception>
	/// <exception cref="TimeoutException">The initial image capture did not complete before <paramref name="timeout"/> expired.</exception>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
	public Task<string> VerifyAsync(GoldenInput actual, TimeSpan timeout,
		CancellationToken cancellationToken = default, GoldenOptions? options = null, TimeSpan? retryInterval = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
		if (retryInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(retryInterval));

		options = GoldenOptions.OrDefault(options);
		var expected = ReferenceInput();
		return RetryAsync(actual, timeout, retryInterval, cancellationToken,
			(image, report) => VerifyCore(this, image, expected, options, report));
	}

	/// <summary>Verifies <paramref name="actual"/> directly against an explicit <paramref name="expected"/> image without a golden store lookup.</summary>
	/// <param name="actual">The captured image path, encoded bytes, or raw pixels to verify.</param>
	/// <param name="expected">The reference image path, encoded bytes, or raw pixels to compare against.</param>
	/// <param name="options">Optional dimension and pixel tolerance overrides; <see langword="null"/> uses <see cref="GoldenOptions"/> defaults.</param>
	/// <returns>The label or file path of the verified <paramref name="actual"/> image.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="actual"/> or <paramref name="expected"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException"><paramref name="actual"/> or <paramref name="expected"/> is an asynchronous capture input or cannot be decoded.</exception>
	public static string Verify(GoldenInput actual, GoldenInput expected, GoldenOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));

		options = GoldenOptions.OrDefault(options);
		var image = actual.Read();
		VerifyCore(null, image, expected.Read(), options);
		return image.Label;
	}

	/// <summary>Repeatedly captures and verifies <paramref name="actual"/> directly against an explicit <paramref name="expected"/> image until it matches or <paramref name="timeout"/> expires.</summary>
	/// <param name="actual">The image input or asynchronous capture callback to invoke on each attempt.</param>
	/// <param name="expected">The reference image path, encoded bytes, or raw pixels to compare against.</param>
	/// <param name="timeout">The total time budget for capturing a matching image.</param>
	/// <param name="cancellationToken">A token to cancel waiting or image capture.</param>
	/// <param name="options">Optional dimension and pixel tolerance overrides; <see langword="null"/> uses <see cref="GoldenOptions"/> defaults.</param>
	/// <param name="retryInterval">The delay between retry captures after the first confirmation retry; <see langword="null"/> defaults to 100 ms.</param>
	/// <returns>A task that resolves to the label or file path of the verified <paramref name="actual"/> image.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="actual"/> or <paramref name="expected"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> or <paramref name="retryInterval"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</exception>
	/// <exception cref="TimeoutException">The initial image capture did not complete before <paramref name="timeout"/> expired.</exception>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
	public static Task<string> VerifyAsync(GoldenInput actual, GoldenInput expected, TimeSpan timeout,
		CancellationToken cancellationToken = default, GoldenOptions? options = null, TimeSpan? retryInterval = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));
		if (retryInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(retryInterval));

		options = GoldenOptions.OrDefault(options);
		GoldenInput.Image? expectedImage = null;
		return RetryAsync(actual, timeout, retryInterval, cancellationToken,
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
		TimeSpan? retryInterval,
		CancellationToken cancellationToken,
		Func<GoldenInput.Image, bool, bool> verify)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

		var delay = retryInterval ?? TimeSpan.FromMilliseconds(100);
		var clock = Stopwatch.StartNew();
		GoldenInput.Image? actual = null;
		var attempts = 0;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var remaining = timeout - clock.Elapsed;
			if (remaining <= TimeSpan.Zero && attempts != 1)
			{
				if (actual == null)
					throw new TimeoutException("The image capture deadline expired before capture completed.");
				verify(actual, true);
				return actual.Label;
			}

			try
			{
				attempts++;
				actual = await WaitAsync(
					input.ReadAsync(),
					attempts == 2 ? timeout : remaining,
					cancellationToken);
			}
			catch (TimeoutException) when (actual != null && !cancellationToken.IsCancellationRequested)
			{
				verify(actual, true);
				return actual.Label;
			}
			if (verify(actual, false))
				return actual.Label;
			if (attempts == 1)
				continue;

			remaining = timeout - clock.Elapsed;
			if (remaining > TimeSpan.Zero)
				await Task.Delay(
					remaining < delay
						? remaining
						: delay,
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
