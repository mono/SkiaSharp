#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Tests;
using SkiaSharp.Views.Maui.Platform;
using UIKit;
using Xunit;

namespace SkiaSharp.Views.Maui.Tests;

public class AppleWheelDeltaTests
{
	[Fact]
	public Task ContactRecognizerRequestsCompletionAfterFinalTouch() =>
		MainThread.InvokeOnMainThreadAsync(() =>
		{
			var actions = new List<SKTouchAction>();
			using var recognizer = new TestTouchGestureRecognizer((action, _, _) =>
			{
				actions.Add(action);
				return true;
			});
			using var view = new UIView();
			view.AddGestureRecognizer(recognizer);
			using var touch = new UITouch();
			using var touches = new NSSet(touch);
			using var evt = new UIEvent();

			recognizer.TouchesBegan(touches, evt);
			Assert.Equal(UIGestureRecognizerState.Possible, recognizer.State);

			recognizer.TouchesEnded(touches, evt);

			Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Released }, actions);
			Assert.Equal(new[] { UIGestureRecognizerState.Failed }, recognizer.RequestedStates);
			Assert.Equal(UIGestureRecognizerState.Possible, recognizer.State);
		});

	[Fact]
	public Task ContactRecognizerCancelsTrackedTouchesWhenReset() =>
		MainThread.InvokeOnMainThreadAsync(() =>
		{
			var actions = new List<SKTouchAction>();
			using var recognizer = new SKTouchHandler.TouchGestureRecognizer((action, _, _) =>
			{
				actions.Add(action);
				return true;
			});
			using var view = new UIView();
			view.AddGestureRecognizer(recognizer);
			using var touch = new UITouch();
			using var touches = new NSSet(touch);
			using var evt = new UIEvent();

			recognizer.TouchesBegan(touches, evt);
			recognizer.Reset();

			Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Cancelled }, actions);
		});

	[Fact]
	public Task ContactRecognizerCancelsTrackedTouchesWhenDisabled() =>
		MainThread.InvokeOnMainThreadAsync(() =>
		{
			var actions = new List<SKTouchAction>();
			using var recognizer = new SKTouchHandler.TouchGestureRecognizer((action, _, _) =>
			{
				actions.Add(action);
				return true;
			});
			using var touch = new UITouch();
			using var touches = new NSSet(touch);
			using var evt = new UIEvent();

			recognizer.TouchesBegan(touches, evt);
			recognizer.CancelTrackedTouches();

			Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Cancelled }, actions);
		});

	[Fact]
	public Task ContactRecognizerAllowsLaterHandledTouch() =>
		MainThread.InvokeOnMainThreadAsync(() =>
		{
			var pressedCount = 0;
			var actions = new List<SKTouchAction>();
			using var recognizer = new TestTouchGestureRecognizer((action, _, _) =>
			{
				actions.Add(action);
				return action != SKTouchAction.Pressed || ++pressedCount > 1;
			});
			using var view = new UIView();
			view.AddGestureRecognizer(recognizer);
			using var firstTouch = new UITouch();
			using var secondTouch = new UITouch();
			using var firstTouches = new NSSet(firstTouch);
			using var secondTouches = new NSSet(secondTouch);
			using var evt = new UIEvent();

			recognizer.TouchesBegan(firstTouches, evt);
			recognizer.TouchesBegan(secondTouches, evt);
			recognizer.TouchesEnded(secondTouches, evt);

			Assert.Equal(
				new[] { SKTouchAction.Pressed, SKTouchAction.Pressed, SKTouchAction.Released },
				actions);
			Assert.Equal(new[] { UIGestureRecognizerState.Failed }, recognizer.RequestedStates);
		});

	[Fact]
	public Task ContactRecognizerRemovesTouchBeforeReleasedCallback() =>
		MainThread.InvokeOnMainThreadAsync(() =>
		{
			var actions = new List<SKTouchAction>();
			SKTouchHandler.TouchGestureRecognizer? recognizer = null;
			using (recognizer = new SKTouchHandler.TouchGestureRecognizer((action, _, _) =>
			{
				actions.Add(action);
				if (action == SKTouchAction.Released)
					recognizer.Reset();
				return true;
			}))
			using (var touch = new UITouch())
			using (var touches = new NSSet(touch))
			using (var evt = new UIEvent())
			{
				recognizer.TouchesBegan(touches, evt);
				recognizer.TouchesEnded(touches, evt);
			}

			Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Released }, actions);
		});

	private sealed class TestTouchGestureRecognizer : SKTouchHandler.TouchGestureRecognizer
	{
		public TestTouchGestureRecognizer(Func<SKTouchAction, UITouch, bool, bool> fireEvent)
			: base(fireEvent)
		{
		}

		public List<UIGestureRecognizerState> RequestedStates { get; } = new();

		protected override void RequestState(UIGestureRecognizerState state) =>
			RequestedStates.Add(state);
	}

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

		var touch = Assert.Single(recognizers.OfType<SKTouchHandler.TouchGestureRecognizer>());
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

		Assert.DoesNotContain(touch, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());
		Assert.DoesNotContain(hover, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());
		Assert.DoesNotContain(scroll, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());

		SetTouchEvents(view, true);

		Assert.Contains(touch, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());
		Assert.Contains(hover, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());
		Assert.Contains(scroll, platformView.GestureRecognizers ?? Array.Empty<UIGestureRecognizer>());

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
