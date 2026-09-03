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
	[InlineData(0, 0)]
	public void WheelTranslationUsesIncrementalV120Units(double translationY, int expected)
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(expected, projector.Project(translationY, UIGestureRecognizerState.Began));
	}

	[Fact]
	public void FractionalWheelTranslationIsPreservedAcrossCallbacks()
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(0, projector.Project(-0.1, UIGestureRecognizerState.Began));
		Assert.Equal(0, projector.Project(-0.1, UIGestureRecognizerState.Changed));
		Assert.Equal(0, projector.Project(-0.1, UIGestureRecognizerState.Changed));
		Assert.Equal(1, projector.Project(-0.1, UIGestureRecognizerState.Changed));
	}

	[Theory]
	[InlineData(-1, -0.1, 3)]
	[InlineData(1, 0.1, -3)]
	public void WheelProjectionIsInvariantToCallbackPartitioning(
		double singleTranslation,
		double partitionedTranslation,
		int expected)
	{
		var singleCallback = new SKTouchHandler.LegacyWheelDeltaProjector();
		var partitionedCallbacks = new SKTouchHandler.LegacyWheelDeltaProjector();

		var singleTotal = singleCallback.Project(singleTranslation, UIGestureRecognizerState.Began);
		var partitionedTotal = partitionedCallbacks.Project(partitionedTranslation, UIGestureRecognizerState.Began);
		for (var i = 1; i < 10; i++)
			partitionedTotal += partitionedCallbacks.Project(partitionedTranslation, UIGestureRecognizerState.Changed);

		Assert.Equal(expected, singleTotal);
		Assert.Equal(singleTotal, partitionedTotal);
	}

	[Fact]
	public void EndedTranslationIsProjectedBeforeRemainderIsDropped()
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(0, projector.Project(-0.25, UIGestureRecognizerState.Began));
		Assert.Equal(2, projector.Project(-0.5, UIGestureRecognizerState.Changed));
		Assert.Equal(1, projector.Project(-0.25, UIGestureRecognizerState.Ended));
		Assert.Equal(0, projector.Project(0, UIGestureRecognizerState.Began));
	}

	[Fact]
	public void EndedOnlyTranslationIsProjected()
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(3, projector.Project(-1, UIGestureRecognizerState.Ended));
	}

	[Fact]
	public void SubUnitTranslationDoesNotForceWheelDelta()
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(0, projector.Project(-0.1, UIGestureRecognizerState.Began));
		Assert.Equal(0, projector.Project(0, UIGestureRecognizerState.Ended));
	}

	[Fact]
	public void ProjectorsDoNotShareRemainders()
	{
		var first = new SKTouchHandler.LegacyWheelDeltaProjector();
		var second = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(0, first.Project(-0.3, UIGestureRecognizerState.Began));
		Assert.Equal(0, second.Project(-0.1, UIGestureRecognizerState.Began));
		Assert.Equal(1, first.Project(-0.1, UIGestureRecognizerState.Changed));
		Assert.Equal(0, second.Project(-0.1, UIGestureRecognizerState.Changed));
	}

	[Fact]
	public void ResetDropsCancelledGestureRemainder()
	{
		var projector = new SKTouchHandler.LegacyWheelDeltaProjector();

		Assert.Equal(0, projector.Project(-0.3, UIGestureRecognizerState.Began));
		projector.Reset();
		Assert.Equal(0, projector.Project(-0.1, UIGestureRecognizerState.Changed));
	}
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
		Assert.Empty(scroll.AllowedTouchTypes);
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
