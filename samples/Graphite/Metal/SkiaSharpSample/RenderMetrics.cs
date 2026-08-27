using System.Diagnostics;

namespace SkiaSharpSample;

internal readonly record struct FrameMeasurement(
	double CpuFrameMs,
	double TileWallMs,
	double UiThreadBusyMs,
	bool ContentUpdated);

internal sealed record RenderMetrics(
	double FramesPerSecond,
	double ContentUpdatesPerSecond,
	double CpuFrameMs,
	double TileWallMs,
	double UiThreadBusyMs);

internal sealed class FrameMeter
{
	private readonly Stopwatch interval = Stopwatch.StartNew();
	private int frames;
	private int contentUpdates;
	private double cpuFrameTotal;
	private double tileWallTotal;
	private double uiThreadBusyTotal;

	public RenderMetrics? Add(FrameMeasurement measurement)
	{
		frames++;
		if (measurement.ContentUpdated)
		{
			contentUpdates++;
			tileWallTotal += measurement.TileWallMs;
		}
		cpuFrameTotal += measurement.CpuFrameMs;
		uiThreadBusyTotal += measurement.UiThreadBusyMs;

		if (interval.Elapsed.TotalSeconds < 0.4)
			return null;

		var elapsed = interval.Elapsed.TotalSeconds;
		var result = new RenderMetrics(
			FramesPerSecond: frames / elapsed,
			ContentUpdatesPerSecond: contentUpdates / elapsed,
			CpuFrameMs: cpuFrameTotal / frames,
			TileWallMs: contentUpdates == 0 ? 0 : tileWallTotal / contentUpdates,
			UiThreadBusyMs: uiThreadBusyTotal / frames);

		interval.Restart();
		frames = 0;
		contentUpdates = 0;
		cpuFrameTotal = 0;
		tileWallTotal = 0;
		uiThreadBusyTotal = 0;
		return result;
	}

	public void Reset()
	{
		interval.Restart();
		frames = 0;
		contentUpdates = 0;
		cpuFrameTotal = 0;
		tileWallTotal = 0;
		uiThreadBusyTotal = 0;
	}
}
