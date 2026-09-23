#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

/// <summary>
/// Tests for SKGLView rendering behavior. These tests cover the Android-specific
/// GLTextureView lifecycle, particularly the bug where the view stops rendering
/// after being detached and reattached to the window without a size change.
/// See https://github.com/mono/SkiaSharp/issues/2550 and PR #3076.
/// </summary>
[Collection("SKUITests")]
public class SKGLViewTests : SKUITests
{
	/// <summary>
	/// Verifies that <see cref="SKGLView"/> renders correctly after its window is
	/// detached and reattached — the Android equivalent of switching away from a tab
	/// and switching back.
	///
	/// <para><b>Bug (before PR #3076)</b><br/>
	/// <c>GLTextureView.OnAttachedToWindow()</c> called <c>new GLThread(weakRef)</c>,
	/// hard-coding <c>width=0</c> and <c>height=0</c>. <c>GLThread.IsReadyToDraw()</c>
	/// requires <c>width &gt; 0 &amp;&amp; height &gt; 0</c>, so the thread would never
	/// schedule a frame. Normally <c>OnLayoutChange</c> corrects the dimensions, but in
	/// a real tab-bar switch the view's bounds are <em>unchanged</em>, so Android does
	/// not fire a second layout pass and the view is permanently blank.</para>
	///
	/// <para><b>Fix (PR #3076)</b><br/>
	/// <c>OnAttachedToWindow()</c> now calls
	/// <c>new GLThread(weakRef, Width, Height)</c>, giving the thread the correct pixel
	/// dimensions immediately on construction.</para>
	///
	/// <para><b>Test strategy</b><br/>
	/// High-level MAUI navigation (TabbedPage, modal push/pop) cannot reproduce the
	/// bug reliably because MAUI's fragment lifecycle triggers a layout pass with
	/// updated bounds, which masks the zero-dimension problem. Instead the test
	/// operates at the native Android level — mirroring what Android's Fragment
	/// Manager does during an actual tab switch:<br/>
	/// 1. <c>nativeParent.RemoveView()</c> → <c>OnDetachedFromWindow</c> fires,
	///    GLThread exits, <c>SurfaceTexture</c> is destroyed
	///    (<c>mSurface = null</c>).<br/>
	/// 2. <c>nativeParent.AddView()</c> → <c>OnAttachedToWindow</c> fires
	///    synchronously, creating a new GLThread with either <c>0×0</c> (bug) or
	///    the view's current pixel dimensions (fix).<br/>
	/// 3. The <c>IOnLayoutChangeListener</c> is removed immediately after
	///    <c>addView()</c> returns — before the asynchronous layout pass runs.
	///    Because <c>mSurface == null</c>, <c>TextureView.onSizeChanged()</c> also
	///    cannot call <c>onSurfaceTextureSizeChanged</c>, so there is no secondary
	///    path that could supply correct dimensions.<br/>
	/// 4. The SurfaceTexture is recreated (async). <c>OnSurfaceTextureAvailable</c>
	///    calls <c>OnSurfaceCreated + RequestRender</c> but does <em>not</em> update
	///    the GLThread's dimensions.<br/>
	/// 5. The GL thread evaluates <c>IsReadyToDraw()</c>:<br/>
	///    • Bug  → <c>width=0, height=0</c> → <c>false</c> → no frame → timeout → <b>FAIL</b><br/>
	///    • Fix  → correct dimensions → <c>true</c> → frame painted → <b>PASS</b></para>
	/// </summary>
	[UIFact]
	public async Task SKGLViewRendersAfterReattach()
	{
#if !ANDROID
		// The GLThread / GLTextureView lifecycle is Android-specific.
		throw new SkipException("This test is Android-specific (GLTextureView lifecycle).");
#pragma warning disable CS0162 // Unreachable code detected
		await Task.CompletedTask;
#pragma warning restore CS0162
#else
		// ── Phase 1: initial render (precondition) ─────────────────────────────────
		var renderCount = 0;
		var firstRenderTcs = new TaskCompletionSource();
		var secondRenderTcs = new TaskCompletionSource();

		var glView = new SKGLView
		{
			HeightRequest = 200,
			WidthRequest = 200,
			HasRenderLoop = false,
		};

		glView.PaintSurface += (_, e) =>
		{
			if (e.Info.Width > 0 && e.Info.Height > 0)
			{
				if (Interlocked.Increment(ref renderCount) == 1)
					firstRenderTcs.TrySetResult();
				else
					secondRenderTcs.TrySetResult();
			}
		};

		// Add view to page BEFORE pushing so that the initial layout pass fires
		// while the page is being pushed (SurfaceTexture not yet available) rather
		// than from inside the test, which would block the UI thread.
		var container = new VerticalStackLayout { Children = { glView } };
		var page = new ContentPage { Content = container };
		await CurrentPage.Navigation.PushAsync(page);

		await glView.WaitForLoaded();
		await glView.WaitForLayout();
		glView.InvalidateSurface();

		await Task.WhenAny(firstRenderTcs.Task, Task.Delay(8000));
		Assert.True(firstRenderTcs.Task.IsCompleted,
			"Precondition: SKGLView must render on first display.");

		// ── Phase 2: simulate tab switch (detach + reattach at native level) ───────
		var nativeView = (global::Android.Views.View)glView.Handler!.PlatformView!;
		var nativeParent = (global::Android.Views.ViewGroup)nativeView.Parent!;
		var layoutParams = nativeView.LayoutParameters;

		// GLTextureView implements IOnLayoutChangeListener and registers itself via
		// AddOnLayoutChangeListener(this) during Initialize(). OnLayoutChange calls
		// glThread.OnWindowResize(w, h), which would correct the zero dimensions and
		// mask the bug. We capture the reference so we can temporarily suppress it.
		var layoutListener = (global::Android.Views.View.IOnLayoutChangeListener)nativeView;

		// removeView() fires OnDetachedFromWindow synchronously:
		//   → GLThread.RequestExitAndWait() (blocking, completes before return)
		//   → SurfaceTexture destroyed by TextureView (mSurface = null)
		nativeParent.RemoveView(nativeView);

		// addView() fires OnAttachedToWindow synchronously:
		//   BUG: new GLThread(weakRef)             → width=0,      height=0
		//   FIX: new GLThread(weakRef, Width, Height) → width=correct, height=correct
		nativeParent.AddView(nativeView, layoutParams);

		// Suppress the layout change listener immediately — before the asynchronous
		// Choreographer layout pass runs (~16 ms from now). This prevents
		// OnLayoutChange from calling OnWindowResize and correcting the dimensions,
		// which is exactly what happens in a real tab switch where bounds are unchanged.
		//
		// TextureView.onSizeChanged() cannot call onSurfaceTextureSizeChanged as a
		// fallback because mSurface==null (destroyed above) — it only fires when the
		// existing surface needs resizing, not on recreation.
		nativeView.RemoveOnLayoutChangeListener(layoutListener);

		// The SurfaceTexture is recreated asynchronously. When it arrives,
		// OnSurfaceTextureAvailable → OnSurfaceCreated + RequestRender fires.
		// The GL thread then evaluates IsReadyToDraw():
		//   Bug: width=0 || height=0  → false → no frame scheduled → timeout below
		//   Fix: width>0  && height>0 → true  → PaintSurface fires → secondRenderTcs set

		await Task.WhenAny(secondRenderTcs.Task, Task.Delay(5000));

		// Restore the layout listener before cleanup so normal operation resumes.
		nativeView.AddOnLayoutChangeListener(layoutListener);

		await CurrentPage.Navigation.PopAsync();

		// ── Assertion ──────────────────────────────────────────────────────────────
		Assert.True(secondRenderTcs.Task.IsCompleted,
			"SKGLView did not render after its window was reattached.\n" +
			"Bug (before PR #3076): OnAttachedToWindow created a new GLThread with " +
			"width=0 and height=0. In a real tab-bar switch the view's bounds are " +
			"unchanged, so Android skips the layout pass and OnLayoutChange never " +
			"supplies the correct dimensions. GLThread.IsReadyToDraw() remained false " +
			"and no frame was ever painted.\n" +
			"Fix (PR #3076): pass Width and Height to the GLThread constructor so the " +
			"thread starts with the correct dimensions immediately.");
#endif
	}
}

