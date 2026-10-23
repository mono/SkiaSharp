namespace SkiaSharp.Tests.Shared;

public sealed class GoldenVerifier
{
	private readonly GoldenStore store;

	public GoldenVerifier(GoldenStore store)
	{
		this.store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public GoldenVerification Verify(
		GoldenRequest request,
		ImageSource actual,
		ImageComparisonOptions? options = null) =>
		Verify(store.Resolve(request), actual, options);

	public GoldenVerification Verify(
		GoldenResolution resolution,
		ImageSource actual,
		ImageComparisonOptions? options = null)
	{
		if (resolution == null) throw new ArgumentNullException(nameof(resolution));
		if (actual == null) throw new ArgumentNullException(nameof(actual));

		var actualPng = actual.ReadBytes();
		if (resolution.Key == null)
			return new GoldenVerification(resolution, actual, actualPng, null);

		var expected = ImageSource.FromBytes(resolution.ReadBytes(), resolution.Location!);
		var image = ImageVerifier.Verify(ImageSource.FromBytes(actualPng, actual.Location), expected, options);
		return new GoldenVerification(resolution, actual, actualPng, image);
	}

	public async Task<GoldenVerification> VerifyAsync(
		GoldenRequest request,
		Func<Task<ImageSource>> capture,
		ImageComparisonOptions? options,
		TimeSpan limit,
		CancellationToken cancellationToken)
	{
		if (request == null) throw new ArgumentNullException(nameof(request));
		if (capture == null) throw new ArgumentNullException(nameof(capture));

		var resolution = store.Resolve(request);
		return await ImageCapture.RetryAsync(
			async () => Verify(resolution, await capture(), options),
			limit,
			(verification, report) => verification.IsMatch || verification.IsMissing || report,
			cancellationToken);
	}
}
