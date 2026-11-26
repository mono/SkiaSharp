using System.Reflection;
using SkiaSharpSample.Samples;

namespace SkiaSharpSample.Services;

public class SampleService 
{
	private static readonly Func<SampleBase>[] SampleFactories =
	[
		() => new AnimatedWebpEncoderSample(),
		() => new BlendModesSample(),
		() => new BlurImageFilterSample(),
		() => new CanvasTransformsSample(),
		() => new ColorFontSample(),
		() => new CreateXpsSample(),
		() => new FillPathSample(),
		() => new GifPlayerSample(),
		() => new GradientSample(),
		() => new HeroImageSample(),
		() => new ImageDecoderSample(),
		() => new ImageFilterCropSample(),
		() => new LottiePlayerSample(),
		() => new MeshFlagSample(),
		() => new MeshGlobeSample(),
		() => new MeshSample(),
		() => new NinePatchSample(),
		() => new OverdrawVisualizationSample(),
		() => new PaintFastBoundsSample(),
		() => new PathBuilderSample(),
		() => new PathEffectsSamplerSample(),
		() => new PdfComposerSample(),
		() => new PerlinNoiseTexturesSample(),
		() => new PhotoLabSample(),
		() => new ShaderCrossFadeSample(),
		() => new ShaderPlaygroundSample(),
		() => new SmartTextUnderlineSample(),
		() => new TextLabSample(),
		() => new TextOnPathSample(),
		() => new TextToPathSample(),
		() => new ThreeDSample(),
		() => new VariableFontSample(),
		() => new VectorArtSample(),
		() => new VertexMeshSample(),
		() => new WideGamutP3Sample(),
		() => new WorldTextSample(),
	];

	private readonly SampleBase[] samples;
	private readonly SampleBase[] allSamples;
	private readonly Dictionary<string, Func<SampleBase>> supportedFactories;

	public SampleService()
	{
		allSamples = new SampleBase[SampleFactories.Length];
		var supported = new List<SampleBase>(SampleFactories.Length);
		supportedFactories = new Dictionary<string, Func<SampleBase>>(StringComparer.Ordinal);

		for (var i = 0; i < SampleFactories.Length; i++)
		{
			var factory = SampleFactories[i];
			var sample = factory();
			allSamples[i] = sample;
			if (sample.IsSupported)
			{
				supported.Add(sample);
				supportedFactories[sample.Title] = factory;
			}
		}

		samples = supported.ToArray();

		SkiaSharpVersion = GetAssemblyVersion<SkiaSharp.SKSurface>();
		HarfBuzzSharpVersion = GetAssemblyVersion<HarfBuzzSharp.Blob>();
		BuildTimestamp = GetBuildTimestamp();
		BuildFooter = GetAssemblyMetadata("BuildFooter");
	}

	public string SkiaSharpVersion { get; }

	public string HarfBuzzSharpVersion { get; }

	public DateTimeOffset? BuildTimestamp { get; }

	public string? BuildFooter { get; }

	public IEnumerable<SampleBase> GetSamples() => samples;

	public IEnumerable<SampleBase> GetAllSamples() => allSamples;

	public SampleBase? GetSample(string title) =>
		samples.FirstOrDefault(s => s.Title == title);

	public SampleBase? CreateSample(string title) =>
		title is not null && supportedFactories.TryGetValue(title, out var factory)
			? factory()
			: null;

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

	private static string? GetAssemblyMetadata(string key)
	{
		var attr = typeof(SampleBase).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(a => a.Key == key);
		return string.IsNullOrEmpty(attr?.Value) ? null : attr.Value;
	}
}
