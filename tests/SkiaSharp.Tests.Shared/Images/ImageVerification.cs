namespace SkiaSharp.Tests.Shared;

public sealed class ImageVerification
{
	internal ImageVerification(
		ImageSource actual,
		ImageSource expected,
		byte[] actualPng,
		byte[] expectedPng,
		SKSizeI actualSize,
		SKSizeI expectedSize,
		ImageComparison? comparison,
		byte[]? differencePng,
		string? failure)
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
	}

	public ImageSource Actual { get; }
	public ImageSource Expected { get; }
	public byte[] ActualPng { get; }
	public byte[] ExpectedPng { get; }
	public SKSizeI ActualSize { get; }
	public SKSizeI ExpectedSize { get; }
	public ImageComparison? Comparison { get; }
	public byte[]? DifferencePng { get; }
	public string? Failure { get; }
	public bool IsMatch => Failure == null && Comparison?.IsMatch == true;
}
