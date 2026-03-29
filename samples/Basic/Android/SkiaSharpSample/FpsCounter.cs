using System.Diagnostics;

namespace SkiaSharpSample;

/// <summary>
/// Lightweight frame-rate counter. Call <see cref="Tick"/> once per frame;
/// it returns the measured FPS each time the sampling interval elapses.
/// Also exposes <see cref="ElapsedSeconds"/> so GPU pages can feed shader time
/// from the same clock without a separate <see cref="Stopwatch"/>.
/// </summary>
public sealed class FpsCounter
{
	private readonly Stopwatch stopwatch = new();

	private int frameCount;
	private double lastSampleTime;

	/// <summary>
	/// Seconds between FPS samples (default 0.5 s).
	/// </summary>
	public double Interval { get; set; } = 0.5;

	/// <summary>
	/// Wall-clock seconds since <see cref="Start"/> was called.
	/// Useful as a shader iTime uniform.
	/// </summary>
	public float ElapsedSeconds => (float)stopwatch.Elapsed.TotalSeconds;

	public void Start() => stopwatch.Start();

	public void Stop() => stopwatch.Stop();

	/// <summary>
	/// Record one frame. Returns the measured FPS when the sampling
	/// interval has elapsed, or <c>null</c> otherwise.
	/// </summary>
	public double? Tick()
	{
		frameCount++;
		var now = stopwatch.Elapsed.TotalSeconds;
		var delta = now - lastSampleTime;
		if (delta < Interval)
			return null;

		var fps = frameCount / delta;
		frameCount = 0;
		lastSampleTime = now;
		return fps;
	}
}
