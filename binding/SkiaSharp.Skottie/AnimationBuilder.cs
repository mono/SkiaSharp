using System;
using System.IO;
using SkiaSharp.Resources;

namespace SkiaSharp.Skottie
{
	/// <summary>Provides a builder pattern for creating animations with custom configuration.</summary>
	/// <remarks><para>Use the builder to configure custom font managers and resource providers before loading an animation.</para><para>Create a builder using <see cref="M:SkiaSharp.Skottie.Animation.CreateBuilder(SkiaSharp.Skottie.AnimationBuilderFlags)" />.</para></remarks>
	public sealed unsafe class AnimationBuilder : SKObject, ISKSkipObjectRegistration
	{
		internal AnimationBuilder (AnimationBuilderFlags flags)
			: this (SkottieApi.skottie_animation_builder_new (flags), true)
		{
		}

		internal AnimationBuilder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Sets the font manager used for text rendering in the animation.</summary>
		/// <param name="fontManager">The font manager to use for text rendering.</param>
		/// <returns>This builder instance for method chaining.</returns>
		/// <remarks>Use this method to provide custom fonts for animations that contain text layers.</remarks>
		public AnimationBuilder SetFontManager (SKFontManager fontManager)
		{
			_ = fontManager ?? throw new ArgumentNullException (nameof (fontManager));
			SkottieApi.skottie_animation_builder_set_font_manager (Handle, fontManager.Handle);
			GC.KeepAlive (this);
			GC.KeepAlive (fontManager);
			Referenced (this, fontManager);
			return this;
		}

		/// <summary>Sets the resource provider used for loading external assets referenced by the animation.</summary>
		/// <param name="resourceProvider">The resource provider to use for loading external assets.</param>
		/// <returns>This builder instance for method chaining.</returns>
		/// <remarks>Use this method to provide custom resource loading for animations that reference external files such as images.</remarks>
		public AnimationBuilder SetResourceProvider (ResourceProvider resourceProvider)
		{
			_ = resourceProvider ?? throw new ArgumentNullException (nameof (resourceProvider));
			SkottieApi.skottie_animation_builder_set_resource_provider (Handle, resourceProvider.Handle);
			GC.KeepAlive (this);
			GC.KeepAlive (resourceProvider);
			Referenced (this, resourceProvider);
			return this;
		}

		/// <summary>Gets the statistics for the most recent animation build operation.</summary>
		/// <value>Statistics about the animation parsing and loading process.</value>
		/// <remarks>Check this property after calling <see cref="M:SkiaSharp.Skottie.AnimationBuilder.Build(System.String)" /> to get timing and size information about the build process.</remarks>
		public AnimationBuilderStats Stats
		{
			get
			{
				AnimationBuilderStats stats;
				SkottieApi.skottie_animation_builder_get_stats (Handle, &stats);
				GC.KeepAlive (this);
				return stats;
			}
		}

		/// <summary>Builds an animation from a .NET stream containing Lottie JSON data.</summary>
		/// <param name="stream">A .NET stream containing Lottie animation JSON data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the stream could not be parsed.</returns>
		/// <remarks />
		public Animation? Build (Stream stream)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return Build (data);
		}

		/// <summary>Builds an animation from a Skia stream containing Lottie JSON data.</summary>
		/// <param name="stream">A Skia stream containing Lottie animation JSON data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the stream could not be parsed.</returns>
		/// <remarks />
		public Animation? Build (SKStream stream)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return Build (data);
		}

		/// <summary>Builds an animation from Lottie JSON data.</summary>
		/// <param name="data">The Lottie animation data in JSON format.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the data could not be parsed.</returns>
		/// <remarks />
		public Animation? Build (SKData data)
		{
			_ = data ?? throw new ArgumentNullException (nameof (data));

			var preamble = Utils.GetPreambleSize (data);
			var span = data.AsSpan ().Slice (preamble);

			fixed (byte* ptr = span) {
				try {
					return Animation.GetObject (SkottieApi.skottie_animation_builder_make_from_data (Handle, ptr, (IntPtr)span.Length));
				} finally {
					GC.KeepAlive (this);
					GC.KeepAlive(data);
				}
			}
		}

		/// <summary>Builds an animation from a Lottie JSON file.</summary>
		/// <param name="path">The path to a Lottie animation JSON file.</param>
		/// <returns>A new <see cref="T:SkiaSharp.Skottie.Animation" /> instance, or <see langword="null" /> if the file could not be loaded or parsed.</returns>
		/// <remarks />
		public Animation? Build (string path)
		{
			_ = path ?? throw new ArgumentNullException (nameof (path));

			using var data = SKData.Create (path);
			return Build (data);
		}

		/// <summary>Disposes of the native resources associated with this builder.</summary>
		/// <remarks>Called when the builder is disposed.</remarks>
		protected override void DisposeNative ()
			=> SkottieApi.skottie_animation_builder_delete (Handle);
	}
}
