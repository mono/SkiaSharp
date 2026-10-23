namespace SkiaSharp.Tests.Shared;

public sealed class GoldenVerification
{
	internal GoldenVerification(
		GoldenImageSource actual,
		GoldenImageSource? expected,
		byte[] actualPng,
		byte[]? expectedPng,
		SKSizeI? actualSize,
		SKSizeI? expectedSize,
		SKPixelComparisonResult? comparison,
		byte[]? differencePng,
		string? failure,
		bool isMatch,
		GoldenResolution? resolution = null)
	{
		Actual = actual;
		Expected = expected;
		ActualPng = actualPng;
		ExpectedPng = expectedPng;
		ActualSize = actualSize;
		ExpectedSize = expectedSize;
		Comparison = comparison;
		DifferencePng = differencePng;
		Failure = failure;
		IsMatch = isMatch;
		Resolution = resolution;
	}

	public GoldenImageSource Actual { get; }
	public GoldenImageSource? Expected { get; }
	public byte[] ActualPng { get; }
	public byte[]? ExpectedPng { get; }
	public SKSizeI? ActualSize { get; }
	public SKSizeI? ExpectedSize { get; }
	public SKPixelComparisonResult? Comparison { get; }
	public byte[]? DifferencePng { get; }
	public string? Failure { get; }
	public bool IsMatch { get; }
	public GoldenResolution? Resolution { get; }
	public bool IsMissing => Resolution != null && Resolution.Key == null;

	public IEnumerable<string> Markers() =>
		Resolution == null
			? Array.Empty<string>()
			: GoldenImage.Markers(Resolution, ActualPng, DifferencePng);
}
