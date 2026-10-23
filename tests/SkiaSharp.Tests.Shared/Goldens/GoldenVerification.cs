namespace SkiaSharp.Tests.Shared;

public sealed class GoldenVerification
{
	internal GoldenVerification(
		GoldenResolution resolution,
		ImageSource actual,
		byte[] actualPng,
		ImageVerification? image)
	{
		Resolution = resolution;
		Actual = actual;
		ActualPng = actualPng;
		Image = image;
	}

	public GoldenResolution Resolution { get; }
	public ImageSource Actual { get; }
	public byte[] ActualPng { get; }
	public ImageVerification? Image { get; }
	public bool IsMissing => Resolution.Key == null;
	public bool IsMatch => Image?.IsMatch == true;
	public string Failure =>
		IsMissing ? "Missing reviewed golden. " + Resolution.Describe() :
		Image?.Failure ?? "Golden comparison did not complete.";

	public IEnumerable<string> Markers() =>
		GoldenImage.Markers(Resolution, ActualPng, Image?.DifferencePng);
}
