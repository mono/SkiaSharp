using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

public class MemoryLeakTests : SKUITests
{
	[UIFact]
	public Task SKCanvasViewHandlerDoesNotLeak() =>
		AssertHandlerDoesNotLeak(() =>
		{
			var view = new SKCanvasView();
			view.PaintSurface += (sender, e) =>
			{
				e.Surface.Canvas.Clear(SKColors.Red);
			};
			view.EnableTouchEvents = true;
			view.Touch += (sender, e) =>
			{
				view.InvalidateSurface();
			};
			return view;
		});

	[UIFact]
	public Task SKGLViewHandlerDoesNotLeak() =>
		AssertHandlerDoesNotLeak(() =>
		{
			var view = new SKGLView();
			view.PaintSurface += (sender, e) =>
			{
				e.Surface.Canvas.Clear(SKColors.Red);
			};
			view.EnableTouchEvents = true;
			view.Touch += (sender, e) =>
			{
				view.InvalidateSurface();
			};
			view.HasRenderLoop = true;
			return view;
		});

	private async Task AssertHandlerDoesNotLeak(Func<View> ctor)
	{
		async Task<(WeakReference, WeakReference, WeakReference)> RunTest()
		{
			var view = ctor();
			var page = new ContentPage
			{
				Content = view
			};

			await CurrentPage.Navigation.PushAsync(page);

			await view.WaitForLoaded();
			await view.WaitForLayout();

			var viewReference = new WeakReference(view);
			var handlerReference = new WeakReference(view.Handler);
			var platformViewReference = new WeakReference(view.Handler.PlatformView);

			await page.Navigation.PopAsync();

			return (viewReference, handlerReference, platformViewReference);
		}
		var (viewRef, handlerRef, platformRef) = await RunTest();

		await AssertEx.EventuallyGC(viewRef, handlerRef, platformRef);
	}
}
