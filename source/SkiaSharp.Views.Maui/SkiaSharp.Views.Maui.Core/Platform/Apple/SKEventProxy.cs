using System;

namespace SkiaSharp.Views.Maui.Platform;

internal class SKEventProxy<TVirtualView, TPlatformView>
	where TVirtualView : class
	where TPlatformView : class
{
	private WeakReference<TVirtualView>? virtualView;

	protected TVirtualView? VirtualView =>
		virtualView is not null && virtualView.TryGetTarget(out var v) ? v : null;

	public void Connect(TVirtualView virtualView, TPlatformView platformView)
	{
		this.virtualView = new(virtualView);
		OnConnect(virtualView, platformView);
	}

	protected virtual void OnConnect(TVirtualView virtualView, TPlatformView platformView)
	{
	}

	public void Disconnect(TPlatformView platformView)
	{
		virtualView = null;
		OnDisconnect(platformView);
	}

	protected virtual void OnDisconnect(TPlatformView platformView)
	{
	}
}
