namespace SkiaSharp.Tests.Shared;

public static class ImageVerifier
{
	public static ImageVerification Verify(
		ImageSource actual,
		ImageSource expected,
		ImageComparisonOptions? options = null)
	{
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		if (expected == null) throw new ArgumentNullException(nameof(expected));
		options ??= new ImageComparisonOptions();

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

		var comparison = GoldenImage.Compare(expectedPng, actualPng, options, expected.Location, actual.Location);
		var differencePng = GoldenImage.Difference(expectedPng, actualPng, options, expected.Location, actual.Location);
		var failure = comparison.IsMatch ? null : Failure(comparison, options);
		return new ImageVerification(
			actual, expected, actualPng, expectedPng, actualSize, expectedSize, comparison, differencePng, failure);

		ImageVerification Result(string failure) =>
			new(actual, expected, actualPng, expectedPng, actualSize, expectedSize, null, null, failure);
	}

	public static Task<ImageVerification> VerifyAsync(
		Func<Task<ImageSource>> capture,
		ImageSource expected,
		ImageComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken) =>
		ImageCapture.RetryAsync(
			async () => Verify(await capture(), expected, options),
			limit,
			(verification, report) => verification.IsMatch || report,
			cancellationToken);

	private static string Failure(ImageComparison comparison, ImageComparisonOptions options) =>
		options.MaxAverageColorErrorFraction is { } limit
			? $"average RGBA color error {comparison.AverageColorErrorFraction:P3} exceeds {limit:P3}."
			: $"{comparison.Pixels.ErrorPixelCount}/{comparison.Pixels.TotalPixels} pixels differ, exceeding " +
				$"{options.MaxErrorPixelFraction:P3}; max channel delta {comparison.Pixels.MaxChannelDelta}.";
}
