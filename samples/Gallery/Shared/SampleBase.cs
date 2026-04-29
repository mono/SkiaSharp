using System;
using System.Threading.Tasks;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample;

public abstract class SampleBase
{
	public abstract string Title { get; }

	public virtual string Description { get; } = string.Empty;

	public virtual string Category { get; } = SampleManager.General;

	public virtual DateOnly? DateAdded { get; }


	public virtual bool IsSupported => true;

	public bool IsInitialized { get; private set; } = false;

	public virtual IReadOnlyList<SampleControl> Controls => [];

	// Download support — samples that produce downloadable output override these
	public virtual byte[]? DownloadBytes => null;
	public virtual string DownloadFileName => "download.bin";
	public virtual string DownloadMimeType => "application/octet-stream";
	public bool HasDownload => DownloadBytes is { Length: > 0 };

	public virtual void UpdateControl(string id, object value)
	{
		OnControlChanged(id, value);
	}

	protected virtual void OnControlChanged(string id, object value) { }

	public async Task InitAsync()
	{
		if (!IsInitialized)
		{
			await OnInit();

			IsInitialized = true;
		}
	}

	public void Destroy()
	{
		if (IsInitialized)
		{
			OnDestroy();

			IsInitialized = false;
		}
	}

	protected virtual Task OnInit() => Task.CompletedTask;

	protected virtual void OnDestroy() { }

	public virtual bool MatchesFilter(string searchText)
	{
		if (string.IsNullOrWhiteSpace(searchText))
			return true;
		
		return
			Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
			Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
	}
}
