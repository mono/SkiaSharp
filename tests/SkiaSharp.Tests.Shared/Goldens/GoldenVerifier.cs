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

	internal GoldenVerification Evaluate(
		GoldenImageSource actual,
		GoldenImageSource expected,
		GoldenComparisonOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));
		options ??= new GoldenComparisonOptions();
		options.Validate();

		var actualPng = actual.ReadBytes();
		var expectedPng = expected.ReadBytes();
		if (!GoldenImage.TryGetSize(actualPng, out var actualSize))
			throw new InvalidOperationException("Failed to decode image: " + actual.Location + ".");
		if (!GoldenImage.TryGetSize(expectedPng, out var expectedSize))
			throw new InvalidOperationException("Failed to decode image: " + expected.Location + ".");

		if (actualSize != expectedSize)
			return Result("Image dimensions differ: actual " + actualSize + ", expected " + expectedSize + ".");
		if (options.ExpectedSize is { } required && actualSize != required)
			return Result("Image is " + actualSize + ", expected " + required + ".");

		using var actualImage = GoldenImage.Decode(actualPng, actual.Location);
		using var expectedImage = GoldenImage.Decode(expectedPng, expected.Location);
		var comparison = SKPixelComparer.Compare(expectedImage, actualImage, options);
		using var difference = SKPixelComparer.GenerateDifferenceImage(
			expectedImage, actualImage, options.ChannelTolerance);
		var differencePng = GoldenImage.Encode(difference);
		var isMatch = comparison.IsMatch(options);
		var failure = isMatch ? null : comparison.Failure(options);
		return new GoldenVerification(
			actual, expected, actualPng, expectedPng, actualSize, expectedSize,
			comparison, differencePng, failure, isMatch);

		GoldenVerification Result(string failure) =>
			new(actual, expected, actualPng, expectedPng, actualSize, expectedSize, null, null, failure, false);
	}

	internal GoldenVerification Evaluate(
		GoldenRequest request,
		GoldenImageSource actual,
		GoldenComparisonOptions? options = null)
	{
		if (store == null)
			throw new InvalidOperationException("A GoldenStore is required for request-based verification.");
		var resolution = store.Resolve(request);
		var actualPng = actual.ReadBytes();
		if (resolution.Key == null)
			return new GoldenVerification(
				actual, null, actualPng, null,
				GoldenImage.TryGetSize(actualPng, out var actualSize) ? actualSize : null,
				null, null, null, "Missing reviewed golden. " + resolution.Describe(), false, resolution);

		var expected = GoldenImageSource.FromBytes(resolution.ReadBytes(), resolution.Location!);
		var verification = Evaluate(GoldenImageSource.FromBytes(actualPng, actual.Location), expected, options);
		return new GoldenVerification(
			verification.Actual, verification.Expected, verification.ActualPng, verification.ExpectedPng,
			verification.ActualSize, verification.ExpectedSize, verification.Comparison,
			verification.DifferencePng, verification.Failure, verification.IsMatch, resolution);
	}

	internal Task<GoldenVerification> EvaluateAsync(
		Func<Task<GoldenImageSource>> capture,
		GoldenImageSource expected,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken) =>
		ImageCapture.RetryAsync(
			async () => Evaluate(await capture(), expected, options),
			limit,
			(verification, report) => verification.IsMatch || report,
			cancellationToken);

	internal Task<GoldenVerification> EvaluateAsync(
		GoldenRequest request,
		Func<Task<GoldenImageSource>> capture,
		GoldenComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken)
	{
		if (store == null)
			throw new InvalidOperationException("A GoldenStore is required for request-based verification.");
		var resolution = store.Resolve(request);
		return ImageCapture.RetryAsync(
			async () =>
			{
				var actual = await capture();
				var actualPng = actual.ReadBytes();
				if (resolution.Key == null)
					return new GoldenVerification(
						actual, null, actualPng, null,
						GoldenImage.TryGetSize(actualPng, out var actualSize) ? actualSize : null,
						null, null, null, "Missing reviewed golden. " + resolution.Describe(), false, resolution);
				var expected = GoldenImageSource.FromBytes(resolution.ReadBytes(), resolution.Location!);
				var verification = Evaluate(GoldenImageSource.FromBytes(actualPng, actual.Location), expected, options);
				return new GoldenVerification(
					verification.Actual, verification.Expected, verification.ActualPng, verification.ExpectedPng,
					verification.ActualSize, verification.ExpectedSize, verification.Comparison,
					verification.DifferencePng, verification.Failure, verification.IsMatch, resolution);
			},
			limit,
			(verification, report) => verification.IsMatch || verification.IsMissing || report,
			cancellationToken);
	}

	private static string Complete(GoldenVerification verification)
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
			diffPath = Path.ChangeExtension(verification.Actual.Location, ".diff.png");
			if (Path.GetDirectoryName(diffPath) is { Length: > 0 })
			{
				GoldenImage.Write(diffPath, difference);
				TestContext.Current.AddFileAttachment(diffPath, "image/png");
			}
			else
			{
				TestContext.Current.AddAttachment("difference.png", difference, "image/png");
			}
		}

		Assert.Fail(
			$"Golden images differ: {verification.Failure} Expected {verification.Expected!.Location}; " +
			$"actual {verification.Actual.Location}" + (diffPath == null ? "." : $"; diff {diffPath}."));
		return verification.Actual.Location;
	}
}
