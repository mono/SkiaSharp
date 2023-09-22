using System;
using System.IO;
using SkiaSharp.Resources;

namespace SkiaSharp.Skottie
{
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

		public AnimationBuilder SetFontManager (SKFontManager fontManager)
		{
			_ = fontManager ?? throw new ArgumentNullException (nameof (fontManager));
			SkottieApi.skottie_animation_builder_set_font_manager (Handle, fontManager.Handle);
			return this;
		}

		public AnimationBuilder SetResourceProvider (ResourceProvider resourceProvider)
		{
			_ = resourceProvider ?? throw new ArgumentNullException (nameof (resourceProvider));
			SkottieApi.skottie_animation_builder_set_resource_provider (Handle, resourceProvider.Handle);
			return this;
		}

		public AnimationBuilderStats Stats
		{
			get
			{
				AnimationBuilderStats stats;
				SkottieApi.skottie_animation_builder_get_stats (Handle, &stats);
				return stats;
			}
		}

		public Animation? Build (Stream stream)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return Build (data);
		}

		public Animation? Build (SKStream stream)
		{
			_ = stream ?? throw new ArgumentNullException (nameof (stream));

			using var data = SKData.Create (stream);
			return Build (data);
		}

		public Animation? Build (SKData data)
		{
			_ = data ?? throw new ArgumentNullException (nameof (data));

			var preamble = Utils.GetPreambleSize (data);
			var span = data.AsSpan ().Slice (preamble);

			fixed (byte* ptr = span) {
				return Animation.GetObject (SkottieApi.skottie_animation_builder_make_from_data (Handle, ptr, (IntPtr)span.Length));
			}
		}

		public Animation? Build (string path)
		{
			_ = path ?? throw new ArgumentNullException (nameof (path));

			using var data = SKData.Create (path);
			return Build (data);
		}

		protected override void DisposeNative ()
			=> SkottieApi.skottie_animation_builder_delete (Handle);
	}
}
