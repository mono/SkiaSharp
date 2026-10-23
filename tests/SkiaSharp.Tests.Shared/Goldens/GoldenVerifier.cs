using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Tests.Shared;

public sealed class GoldenVerifier
{
	private readonly GoldenStore? store;

	public GoldenVerifier(GoldenStore? store = null)
	{
		this.store = store;
	}

	public string Verify(
		GoldenImageSource actual,
		GoldenImageSource expected,
		GoldenComparisonOptions? options = null) =>
		Complete(Evaluate(actual, expected, options));

	public string Verify(
		GoldenRequest request,
		GoldenImageSource actual,
		GoldenComparisonOptions? options = null) =>
		Complete(Evaluate(request, actual, options));

	public async Task<string> VerifyAsync(
		Func<Task<GoldenImageSource>> capture,
		GoldenImageSource expected,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken) =>
		Complete(await EvaluateAsync(capture, expected, options, limit, cancellationToken));

	public async Task<string> VerifyAsync(
		GoldenRequest request,
		Func<Task<GoldenImageSource>> capture,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken) =>
		Complete(await EvaluateAsync(request, capture, options, limit, cancellationToken));

	private Verification Evaluate(
		GoldenImageSource actual,
		GoldenImageSource expected,
		GoldenComparisonOptions? options = null)
	{
		if (expected == null) throw new ArgumentNullException(nameof(expected));
		options = ReadActual(actual, options, out var actualPng, out var actualSize);
		return Evaluate(actual, actualPng, actualSize, expected, options);
	}

	private Verification Evaluate(
		GoldenImageSource actual,
		byte[] actualPng,
		SKSizeI actualSize,
		GoldenImageSource expected,
		GoldenComparisonOptions options)
	{
		var expectedPng = expected.ReadBytes();
		if (!GoldenImage.TryGetSize(expectedPng, out var expectedSize))
			throw new InvalidOperationException("Failed to decode image: " + expected.Location + ".");

		if (actualSize != expectedSize)
			return Result("Image dimensions differ: actual " + Size(actualSize) +
				", expected " + Size(expectedSize) + ".");
		if (options.ExpectedSize is { } required && actualSize != required)
			return Result("Image is " + Size(actualSize) + ", expected " + Size(required) + ".");

		using var actualImage = GoldenImage.Decode(actualPng, actual.Location);
		using var expectedImage = GoldenImage.Decode(expectedPng, expected.Location);
		var comparison = SKPixelComparer.Compare(expectedImage, actualImage, options);
		using var difference = SKPixelComparer.GenerateDifferenceImage(
			expectedImage, actualImage, options.ChannelTolerance);
		var differencePng = GoldenImage.Encode(difference);
		var isMatch = comparison.IsMatch(options);
		var failure = isMatch ? null : comparison.Failure(options);
		return new Verification(
			actual, expected, actualPng, comparison, differencePng, failure, isMatch);

		Verification Result(string failure) =>
			new(actual, expected, actualPng, null, null, failure, false);
	}

	private Verification Evaluate(
		GoldenRequest request,
		GoldenImageSource actual,
		GoldenComparisonOptions? options = null)
	{
		if (store == null)
			throw new InvalidOperationException("A GoldenStore is required for request-based verification.");
		var resolution = store.Resolve(request);
		return Evaluate(resolution, actual, options);
	}

	private Verification Evaluate(
		GoldenResolution resolution,
		GoldenImageSource actual,
		GoldenComparisonOptions? options)
	{
		options = ReadActual(actual, options, out var actualPng, out var actualSize);
		if (options.ExpectedSize is { } required && actualSize != required)
			return new Verification(
				actual, null, actualPng, null, null,
				"Image is " + Size(actualSize) + ", expected " + Size(required) + ".", false);
		if (resolution.Key == null)
			return new Verification(
				actual, null, actualPng, null, null,
				"Missing reviewed golden. " + resolution.Describe(), false, resolution);

		var expected = GoldenImageSource.FromBytes(resolution.ReadBytes(), resolution.Location!);
		var verification = Evaluate(actual, actualPng, actualSize, expected, options);
		verification.Resolution = resolution;
		return verification;
	}

	private Task<Verification> EvaluateAsync(
		Func<Task<GoldenImageSource>> capture,
		GoldenImageSource expected,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken) =>
		RetryAsync(
			async () => Evaluate(await capture(), expected, options),
			limit,
			(verification, report) => verification.IsMatch || report,
			cancellationToken);

	private Task<Verification> EvaluateAsync(
		GoldenRequest request,
		Func<Task<GoldenImageSource>> capture,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken)
	{
		if (store == null)
			throw new InvalidOperationException("A GoldenStore is required for request-based verification.");
		var resolution = store.Resolve(request);
		return RetryAsync(
			async () => Evaluate(resolution, await capture(), options),
			limit,
			(verification, report) => verification.IsMatch || verification.IsMissing || report,
			cancellationToken);
	}

	private static GoldenComparisonOptions ReadActual(
		GoldenImageSource actual,
		GoldenComparisonOptions? options,
		out byte[] actualPng,
		out SKSizeI actualSize)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		options ??= new GoldenComparisonOptions();
		options.Validate();
		actualPng = actual.ReadBytes();
		if (!GoldenImage.TryGetSize(actualPng, out actualSize))
			throw new InvalidOperationException("Failed to decode image: " + actual.Location + ".");
		return options;
	}

	private static string Size(SKSizeI size) =>
		size.Width + "x" + size.Height;

	private static async Task<T> RetryAsync<T>(
		Func<Task<T>> capture,
		TimeSpan limit,
		Func<T, bool, bool> compare,
		CancellationToken cancellationToken)
	{
		if (capture == null) throw new ArgumentNullException(nameof(capture));
		if (compare == null) throw new ArgumentNullException(nameof(compare));
		if (limit <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(limit));

		var clock = Stopwatch.StartNew();
		var hasActual = false;
		T actual = default!;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var remaining = limit - clock.Elapsed;
			if (remaining <= TimeSpan.Zero)
			{
				if (!hasActual)
					throw new TimeoutException("The image capture deadline expired before capture completed.");
				compare(actual, true);
				return actual;
			}

			actual = await WaitAsync(capture(), remaining, cancellationToken);
			hasActual = true;
			if (compare(actual, false))
				return actual;

			remaining = limit - clock.Elapsed;
			if (remaining > TimeSpan.Zero)
				await Task.Delay(
					remaining < TimeSpan.FromMilliseconds(100) ? remaining : TimeSpan.FromMilliseconds(100),
					cancellationToken);
		}
	}

	private static async Task<T> WaitAsync<T>(
		Task<T> task,
		TimeSpan timeout,
		CancellationToken cancellationToken)
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

	private sealed class Verification
	{
		public Verification(
			GoldenImageSource actual,
			GoldenImageSource? expected,
			byte[] actualPng,
			SKPixelComparisonResult? comparison,
			byte[]? differencePng,
			string? failure,
			bool isMatch,
			GoldenResolution? resolution = null)
		{
			Actual = actual;
			Expected = expected;
			ActualPng = actualPng;
			Comparison = comparison;
			DifferencePng = differencePng;
			Failure = failure;
			IsMatch = isMatch;
			Resolution = resolution;
		}

		public GoldenImageSource Actual { get; }
		public GoldenImageSource? Expected { get; }
		public byte[] ActualPng { get; }
		public SKPixelComparisonResult? Comparison { get; }
		public byte[]? DifferencePng { get; }
		public string? Failure { get; }
		public bool IsMatch { get; }
		public GoldenResolution? Resolution { get; set; }
		public bool IsMissing => Resolution != null && Resolution.Key == null;

		public IEnumerable<string> Markers() =>
			Resolution == null
				? Array.Empty<string>()
				: GoldenImage.Markers(Resolution, ActualPng, DifferencePng);
	}

	private static string Complete(Verification verification)
	{
		if (verification.Resolution != null)
		{
			TestContext.Current.TestOutputHelper?.WriteLine(verification.Resolution.Describe());
			foreach (var marker in verification.Markers())
				TestContext.Current.TestOutputHelper?.WriteLine(marker);
		}

		TestContext.Current.AddImageAttachment(verification.Actual);
		if (verification.IsMissing)
			Assert.Fail(verification.Failure + $" Actual retained at {verification.Actual.Location}.");
		if (verification.Expected == null)
			Assert.Fail(verification.Failure);

		if (verification.Comparison is { } comparison)
		{
			TestContext.Current.TestOutputHelper?.WriteLine(
				$"Image comparison: {comparison.ErrorPixelCount}/{comparison.TotalPixels} pixels differ; " +
				$"average RGBA color error {comparison.AverageColorErrorFraction:P3}.");
		}
		if (verification.IsMatch)
			return verification.Actual.Location;

		TestContext.Current.AddImageAttachment(
			verification.Expected!, $"expected-{Path.GetFileName(verification.Expected!.Location)}");
		string? diffPath = null;
		if (verification.DifferencePng is { } difference)
		{
			if (File.Exists(verification.Actual.Location))
			{
				diffPath = Path.ChangeExtension(verification.Actual.Location, ".diff.png");
				GoldenImage.Write(diffPath, difference);
				TestContext.Current.AddFileAttachment(diffPath, "image/png");
			}
			else
			{
				TestContext.Current.AddImageAttachment(
					GoldenImageSource.FromBytes(difference, "difference.png"));
			}
		}

		Assert.Fail(
			$"Golden images differ: {verification.Failure} Expected {verification.Expected!.Location}; " +
			$"actual {verification.Actual.Location}" + (diffPath == null ? "." : $"; diff {diffPath}."));
		return verification.Actual.Location;
	}
}
