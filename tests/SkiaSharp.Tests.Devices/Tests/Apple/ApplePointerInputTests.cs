#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Tests;
using SkiaSharp.Views.Maui.Platform;
using UIKit;
using Xunit;

namespace SkiaSharp.Views.Maui.Tests;

public class AppleWheelDeltaTests
{
	[Theory]
	[InlineData(-40, 120)]
	[InlineData(40, -120)]
	[InlineData(-2, 6)]
	[InlineData(2, -6)]
	[InlineData(-0.1, 1)]
	[InlineData(0.1, -1)]
	[InlineData(0, 0)]
	public void WheelTranslationUsesIncrementalV120Units(double translationY, int expected) =>
		Assert.Equal(expected, SKTouchHandler.NormalizeWheelDelta(translationY));
}

[Collection("SKUITests")]
public class ApplePointerInputTests : SKUITests
{
	[UIFact]
	public Task SKCanvasViewAddsPointerRecognizers() =>
		AssertPointerRecognizers(new SKCanvasView());

	[UIFact]
	public Task SKGLViewAddsPointerRecognizers() =>
		AssertPointerRecognizers(new SKGLView());

	private async Task AssertPointerRecognizers(View view)
	{
		SetTouchEvents(view, true);
		var page = new ContentPage
		{
			Content = view,
		};

		await CurrentPage.Navigation.PushAsync(page);
		await view.WaitForLoaded();
		await view.WaitForLayout();

		var platformView = Assert.IsAssignableFrom<UIView>(view.Handler!.PlatformView);
		var recognizers = platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>();

		var hover = Assert.Single(recognizers.OfType<UIHoverGestureRecognizer>());
		Assert.False(hover.CancelsTouchesInView);

		var scroll = Assert.Single(recognizers
			.OfType<UIPanGestureRecognizer>()
			.Where(recognizer =>
				recognizer.AllowedScrollTypesMask == UIScrollTypeMask.All &&
				recognizer.MaximumNumberOfTouches == 0));
		Assert.False(scroll.CancelsTouchesInView);
		Assert.NotNull(scroll.Delegate);

		using var pinch = new UIPinchGestureRecognizer();
		using var tap = new UITapGestureRecognizer();
		Assert.True(scroll.Delegate.ShouldRecognizeSimultaneously(scroll, pinch));
		Assert.False(scroll.Delegate.ShouldRecognizeSimultaneously(scroll, tap));

		SetTouchEvents(view, false);

		Assert.DoesNotContain(hover, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());
		Assert.DoesNotContain(scroll, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());

		await CurrentPage.Navigation.PopAsync();
	}

	private static void SetTouchEvents(View view, bool enabled)
	{
		switch (view)
		{
			case SKCanvasView canvasView:
				canvasView.EnableTouchEvents = enabled;
				break;
			case SKGLView glView:
				glView.EnableTouchEvents = enabled;
				break;
			default:
				throw new ArgumentException("Expected a SkiaSharp view.", nameof(view));
		}
	}
}
