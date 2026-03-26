namespace SkiaSharpSample.Services;

public interface ISampleService
{
	IEnumerable<SampleBase> GetSamples();
	SampleBase? GetSample(string title);
	string SkiaSharpVersion { get; }
	string HarfBuzzSharpVersion { get; }
}
