using System.Reflection;

namespace SkiaSharpSample.Services;

public class SampleService 
{
	private readonly SampleBase[] samples;
	private readonly SampleBase[] allSamples;

	public SampleService()
	{
		var samplesBase = typeof(SampleBase).GetTypeInfo();
		var assembly = samplesBase.Assembly;

		allSamples = assembly.DefinedTypes
			.Where(t => samplesBase.IsAssignableFrom(t) && !t.IsAbstract)
			.Select(t => (SampleBase)Activator.CreateInstance(t.AsType())!)
			.ToArray();

		samples = allSamples.Where(s => s.IsSupported).ToArray();

		SkiaSharpVersion = GetAssemblyVersion<SkiaSharp.SKSurface>();
		HarfBuzzSharpVersion = GetAssemblyVersion<HarfBuzzSharp.Blob>();
		BuildTimestamp = GetBuildTimestamp();
	}

	public string SkiaSharpVersion { get; }

	public string HarfBuzzSharpVersion { get; }

	public DateTimeOffset? BuildTimestamp { get; }

	public IEnumerable<SampleBase> GetSamples() => samples;

	public IEnumerable<SampleBase> GetAllSamples() => allSamples;

	public SampleBase? GetSample(string title) =>
		samples.FirstOrDefault(s => s.Title == title);

	private static string GetAssemblyVersion<T>()
	{
		var apiAssembly = typeof(T).Assembly;
		var attributes = apiAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
		var attribute = (AssemblyInformationalVersionAttribute?)attributes.FirstOrDefault();
		return attribute?.InformationalVersion ?? "<unavailable>";
	}

	private static DateTimeOffset? GetBuildTimestamp()
	{
		var assembly = typeof(SampleBase).Assembly;
		var attr = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(a => a.Key == "BuildTimestampUtc");
		if (attr?.Value is { } value && DateTimeOffset.TryParse(value, out var ts))
			return ts;
		return null;
	}
}
