using System.Reflection;

namespace SkiaSharpSample.Services;

public class SampleService 
{
	private readonly SampleBase[] samples;

	public SampleService()
	{
		var samplesBase = typeof(SampleBase).GetTypeInfo();
		var assembly = samplesBase.Assembly;

		samples = assembly.DefinedTypes
			.Where(t => samplesBase.IsAssignableFrom(t) && !t.IsAbstract)
			.Select(t => (SampleBase)Activator.CreateInstance(t.AsType())!)
			.Where(s => s.IsSupported)
			.ToArray();

		SkiaSharpVersion = GetAssemblyVersion<SkiaSharp.SKSurface>();
		HarfBuzzSharpVersion = GetAssemblyVersion<HarfBuzzSharp.Blob>();
	}

	public string SkiaSharpVersion { get; }

	public string HarfBuzzSharpVersion { get; }

	public IEnumerable<SampleBase> GetSamples() => samples;

	public SampleBase? GetSample(string title) =>
		samples.FirstOrDefault(s => s.Title == title);

	private static string GetAssemblyVersion<T>()
	{
		var apiAssembly = typeof(T).Assembly;
		var attributes = apiAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
		var attribute = (AssemblyInformationalVersionAttribute?)attributes.FirstOrDefault();
		return attribute?.InformationalVersion ?? "<unavailable>";
	}
}
