namespace SkiaSharpSample;

internal enum WorkloadKind
{
	UiDashboard,
	VectorTiles,
	SpriteAtlas,
	TextGrid,
}

internal enum DisplayMode
{
	IsolatedAB,
	SideBySideContended,
	Ganesh,
	Graphite,
}

internal sealed record RenderSettings(
	WorkloadKind Workload,
	int WorkerCount,
	int Complexity,
	bool Animate,
	int FramesPerSecond)
{
	public static RenderSettings Default { get; } = new(
		Workload: WorkloadKind.UiDashboard,
		WorkerCount: Math.Clamp(Math.Max(1, Environment.ProcessorCount / 2), 1, 8),
		Complexity: 4_000,
		Animate: true,
		FramesPerSecond: 60);
}

internal sealed class RenderSettingsStore
{
	private readonly object gate = new();
	private RenderSettings current = RenderSettings.Default;

	public RenderSettings Current
	{
		get
		{
			lock (gate)
				return current;
		}
	}

	public void Update(Func<RenderSettings, RenderSettings> update)
	{
		lock (gate)
			current = update(current);
	}
}
