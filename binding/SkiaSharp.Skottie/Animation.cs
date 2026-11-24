using System;
using System.IO;
using SkiaSharp.Resources;
using SkiaSharp.SceneGraph;

namespace SkiaSharp.Skottie
{
	/// <summary>Represents a Lottie animation that can be loaded, positioned, and rendered to a canvas.</summary>
	/// <remarks><para>Skottie is a Lottie animation player built on top of the Skia graphics library. Lottie is an animation file format that allows designers to create animations in tools like Adobe After Effects and export them as JSON files for use in applications.</para><para>Use the <see cref="M:SkiaSharp.Skottie.Animation.Create(System.String)" /> or <see cref="M:SkiaSharp.Skottie.Animation.Parse(System.String)" /> factory methods to load an animation, then use <see cref="M:SkiaSharp.Skottie.Animation.Seek(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> or <see cref="M:SkiaSharp.Skottie.Animation.SeekFrame(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> to set the animation state, and finally call <see cref="M:SkiaSharp.Skottie.Animation.Render(SkiaSharp.SKCanvas,SkiaSharp.SKRect)" /> to draw the current frame.</para><para>For more control over the animation loading process, use <see cref="M:SkiaSharp.Skottie.Animation.CreateBuilder(SkiaSharp.Skottie.AnimationBuilderFlags)" /> to obtain an <see cref="T:SkiaSharp.Skottie.AnimationBuilder" />.</para></remarks>
	public unsafe class Animation : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
	{
		internal Animation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		void ISKNonVirtualReferenceCounted.ReferenceNative ()
			=> SkottieApi.skottie_animation_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
			=> SkottieApi.skottie_animation_unref (Handle);

		/// <summary>Disposes of the native resources associated with this animation.</summary>
		/// <remarks>Called when the animation is disposed.</remarks>
		protected override void DisposeNative ()
			=> SkottieApi.skottie_animation_delete (Handle);

		// AnimationBuilder

		/// <summary>Creates a new animation builder with the specified flags.</summary>
		/// <param name="flags">Builder flags to control the loading process.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.AnimationBuilder" /> instance.</returns>
		/// <remarks>Use the builder for more options and control over the loading process, such as setting a custom font manager or resource provider.</remarks>
		public static AnimationBuilder CreateBuilder (AnimationBuilderFlags flags = AnimationBuilderFlags.None) =>
			new AnimationBuilder (flags);

		// Parse

		/// <summary>Parses an animation from a Lottie JSON string.</summary>
		/// <param name="json">A string containing the Lottie animation JSON.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the JSON could not be parsed.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static Animation? Parse (string json) =>
			TryParse (json, out var animation)
				? animation
				: null;

		/// <summary>Attempts to parse an animation from a Lottie JSON string.</summary>
		/// <param name="json">A string containing the Lottie animation JSON.</param>
		/// <param name="animation">When this method returns, contains the animation, or <see langword="null" /> if the parsing failed.</param>
		/// <returns><see langword="true" /> if the animation was parsed successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static bool TryParse (string json, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = json ?? throw new ArgumentNullException (nameof (json));

			animation = GetObject (SkottieApi.skottie_animation_make_from_string (json, json.Length));
			return animation != null;
		}

		// Create

		/// <summary>Creates an animation from a .NET stream containing Lottie JSON data.</summary>
		/// <param name="stream">A .NET stream containing Lottie animation JSON data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the stream could not be parsed.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static Animation? Create (Stream stream) =>
			TryCreate (stream, out var animation)
				? animation
				: null;

		/// <summary>Attempts to create an animation from a .NET stream containing Lottie JSON data.</summary>
		/// <param name="stream">A .NET stream containing Lottie animation JSON data.</param>
		/// <param name="animation">When this method returns, contains the animation, or <see langword="null" /> if the creation failed.</param>
		/// <returns><see langword="true" /> if the animation was created successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static bool TryCreate (Stream stream, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return TryCreate (data, out animation);
		}

		/// <summary>Creates an animation from a Skia stream containing Lottie JSON data.</summary>
		/// <param name="stream">A Skia stream containing Lottie animation JSON data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the stream could not be parsed.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static Animation? Create (SKStream stream) =>
			TryCreate (stream, out var animation)
				? animation
				: null;

		/// <summary>Attempts to create an animation from a Skia stream containing Lottie JSON data.</summary>
		/// <param name="stream">A Skia stream containing Lottie animation JSON data.</param>
		/// <param name="animation">When this method returns, contains the animation, or <see langword="null" /> if the creation failed.</param>
		/// <returns><see langword="true" /> if the animation was created successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static bool TryCreate (SKStream stream, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return TryCreate (data, out animation);
		}

		/// <summary>Creates an animation from Lottie JSON data.</summary>
		/// <param name="data">The Lottie animation data in JSON format.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the data could not be parsed.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static Animation? Create (SKData data) =>
			TryCreate (data, out var animation)
				? animation
				: null;

		/// <summary>Attempts to create an animation from Lottie JSON data.</summary>
		/// <param name="data">The Lottie animation data in JSON format.</param>
		/// <param name="animation">When this method returns, contains the animation, or <see langword="null" /> if the creation failed.</param>
		/// <returns><see langword="true" /> if the animation was created successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static bool TryCreate (SKData data, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = data ?? throw new ArgumentNullException (nameof (data));

			var preamble = Utils.GetPreambleSize (data);
			var span = data.AsSpan ().Slice (preamble);

			fixed (byte* ptr = span) {
				animation = GetObject (SkottieApi.skottie_animation_make_from_data (ptr, (IntPtr)span.Length));
				GC.KeepAlive(data);
				return animation != null;
			}
		}

		/// <summary>Creates an animation from a Lottie JSON file.</summary>
		/// <param name="path">The path to a Lottie animation JSON file.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the file could not be loaded or parsed.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static Animation? Create (string path) =>
			TryCreate (path, out var animation)
				? animation
				: null;

		/// <summary>Attempts to create an animation from a Lottie JSON file.</summary>
		/// <param name="path">The path to a Lottie animation JSON file.</param>
		/// <param name="animation">When this method returns, contains the animation, or <see langword="null" /> if the creation failed.</param>
		/// <returns><see langword="true" /> if the animation was created successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks>Use the Builder helper for more options and control over the loading process.</remarks>
		public static bool TryCreate (string path, [System.Diagnostics.CodeAnalysis.NotNullWhen (true)] out Animation? animation)
		{
			_ = path ?? throw new ArgumentNullException (nameof (path));

			using var data = SKData.Create (path);
			return TryCreate (data, out animation);
		}

		// Render

		/// <summary>Renders the current animation frame to a canvas.</summary>
		/// <param name="canvas">The canvas to render onto.</param>
		/// <param name="dst">The destination rectangle.</param>
		/// <remarks>The current frame is determined by the most recent <see cref="M:SkiaSharp.Skottie.Animation.Seek(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> or <see cref="M:SkiaSharp.Skottie.Animation.SeekFrame(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> call. The animation is scaled to fit the destination rectangle while preserving the aspect ratio.</remarks>
		public unsafe void Render (SKCanvas canvas, SKRect dst)
		{
			SkottieApi.skottie_animation_render (Handle, canvas.Handle, &dst);
			GC.KeepAlive (this);
			GC.KeepAlive (canvas);
		}

		/// <summary>Renders the current animation frame to a canvas with the specified render flags.</summary>
		/// <param name="canvas">The canvas to render onto.</param>
		/// <param name="dst">The destination rectangle.</param>
		/// <param name="flags">Render flags that control the rendering behavior.</param>
		/// <remarks>The current frame is determined by the most recent <see cref="M:SkiaSharp.Skottie.Animation.Seek(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> or <see cref="M:SkiaSharp.Skottie.Animation.SeekFrame(System.Double,SkiaSharp.SceneGraph.InvalidationController)" /> call. The animation is scaled to fit the destination rectangle while preserving the aspect ratio.</remarks>
		public void Render (SKCanvas canvas, SKRect dst, AnimationRenderFlags flags)
		{
			SkottieApi.skottie_animation_render_with_flags (Handle, canvas.Handle, &dst, flags);
			GC.KeepAlive (this);
			GC.KeepAlive (canvas);
		}

		// Seek*

		/// <summary>Sets the animation state at a normalized position.</summary>
		/// <param name="percent">The normalized position in the animation, where 0.0 is the start and 1.0 is the end.</param>
		/// <param name="ic">An optional invalidation controller that tracks the regions requiring repaint.</param>
		/// <remarks>Values are clamped to the [0, 1] interval. When a seek triggers an invalidation, any dirty regions are reported to the optional invalidation controller.</remarks>
		public void Seek (double percent, InvalidationController? ic = null)
		{
			SkottieApi.skottie_animation_seek (Handle, (float)percent, ic?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (this);
			GC.KeepAlive (ic);
		}

		/// <summary>Sets the animation state at a specific frame number.</summary>
		/// <param name="frame">The frame number to seek to.</param>
		/// <param name="ic">An optional invalidation controller that tracks the regions requiring repaint.</param>
		/// <remarks>The frame number is clamped to the [<see cref="P:SkiaSharp.Skottie.Animation.InPoint" />, <see cref="P:SkiaSharp.Skottie.Animation.OutPoint" />] interval. When a seek triggers an invalidation, any dirty regions are reported to the optional invalidation controller.</remarks>
		public void SeekFrame (double frame, InvalidationController? ic = null)
		{
			SkottieApi.skottie_animation_seek_frame (Handle, (float)frame, ic?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (this);
			GC.KeepAlive (ic);
		}

		/// <summary>Sets the animation state at a specific time position in seconds.</summary>
		/// <param name="seconds">The time in seconds to seek to.</param>
		/// <param name="ic">An optional invalidation controller that tracks the regions requiring repaint.</param>
		/// <remarks>Values are clamped to the animation duration. When a seek triggers an invalidation, any dirty regions are reported to the optional invalidation controller.</remarks>
		public void SeekFrameTime (double seconds, InvalidationController? ic = null)
		{
			SkottieApi.skottie_animation_seek_frame_time (Handle, (float)seconds, ic?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (this);
			GC.KeepAlive (ic);
		}

		/// <summary>Sets the animation state at a specific time position.</summary>
		/// <param name="time">The time to seek to.</param>
		/// <param name="ic">An optional invalidation controller that tracks the regions requiring repaint.</param>
		/// <remarks>Values are clamped to the animation duration. When a seek triggers an invalidation, any dirty regions are reported to the optional invalidation controller.</remarks>
		public void SeekFrameTime (TimeSpan time, InvalidationController? ic = null)
			=> SeekFrameTime (time.TotalSeconds, ic);

		// Properties

		/// <summary>Gets the animation duration.</summary>
		/// <value>The total duration of the animation.</value>
		/// <remarks />
		public TimeSpan Duration {
			get {
				var r = SkottieApi.skottie_animation_get_duration (Handle);
				GC.KeepAlive (this);
				return TimeSpan.FromSeconds (r);
			}
		}

		/// <summary>Gets the animation frame rate.</summary>
		/// <value>The animation frame rate in frames per second (FPS).</value>
		/// <remarks />
		public double Fps {
			get {
				var r = SkottieApi.skottie_animation_get_fps (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the animation "in" point.</summary>
		/// <value>The animation "in" point (time in frames).</value>
		/// <remarks>This is the frame number where the animation playback should start. Seek values are clamped to the [<see cref="P:SkiaSharp.Skottie.Animation.InPoint" />, <see cref="P:SkiaSharp.Skottie.Animation.OutPoint" />] range.</remarks>
		public double InPoint {
			get {
				var r = SkottieApi.skottie_animation_get_in_point (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the animation "out" point.</summary>
		/// <value>The animation "out" point (time in frames).</value>
		/// <remarks>This is the frame number where the animation playback should end. Seek values are clamped to the [<see cref="P:SkiaSharp.Skottie.Animation.InPoint" />, <see cref="P:SkiaSharp.Skottie.Animation.OutPoint" />] range.</remarks>
		public double OutPoint {
			get {
				var r = SkottieApi.skottie_animation_get_out_point (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the animation Lottie bodymovin version.</summary>
		/// <value>The version string from the Lottie JSON file.</value>
		/// <remarks />
		public string Version {
			get {
				using var str = new SKString ();
				SkottieApi.skottie_animation_get_version (Handle, str.Handle);
				GC.KeepAlive (this);
				return str.ToString ();
			}
		}

		/// <summary>Gets the animation size.</summary>
		/// <value>The intrinsic size of the animation.</value>
		/// <remarks />
		public unsafe SKSize Size {
			get {
				SKSize size;
				SkottieApi.skottie_animation_get_size (Handle, &size);
				GC.KeepAlive (this);
				return size;
			}
		}

		internal static Animation? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new Animation (handle, true);
	}
}
