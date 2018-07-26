using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Interactive.Logging;
using Xamarin.Interactive.Representations;
using Xamarin.Interactive.Representations.Reflection;
using Xamarin.Interactive.Serialization;

namespace SkiaSharp.Workbooks
{
	sealed class SkiaSharpRepresentationProvider : RepresentationProvider
	{
		public override IEnumerable<object> ProvideRepresentations (object obj)
		{
			yield return ProvideSingleRepresentation (obj);
		}

		public override bool TryConvertFromRepresentation (
			IRepresentedType representedType,
			object [] representations,
			out object represented)
		{
			represented = null;

			Color color;
			if (TryFindMatchingRepresentation<SKColor, Color> (
				representedType,
				representations,
				out color)) {
				represented = new SKColor (
					(byte)(color.Red * 255),
					(byte)(color.Green * 255),
					(byte)(color.Blue * 255),
					(byte)(color.Alpha * 255));
				return true;
			}
			return base.TryConvertFromRepresentation (representedType, representations, out represented);
		}

		ISerializableObject ProvideSingleRepresentation (object obj)
		{
			try {
				var bitmap = obj as SKBitmap;
				if (bitmap != null)
					return SKImageToRepresentationImage (SKImage.FromBitmap (bitmap));

				var image = obj as SKImage;
				if (image != null)
					return SKImageToRepresentationImage (image);

				var surface = obj as SKSurface;
				if (surface != null)
					return SKImageToRepresentationImage (surface.Snapshot ());

				var pixmap = obj as SKPixmap;
				if (pixmap != null)
					return ImageFromSKData (
						pixmap.Encode (SKEncodedImageFormat.Png, 85),
						pixmap.Width,
						pixmap.Height);

				if (obj is SKColor) {
					var color = (SKColor)obj;
					return new Color (color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, color.Alpha / 255.0);
				}

				return null;
			} catch (Exception e) {
				Log.Error (
					nameof (SkiaSharpRepresentationProvider),
					$"Error while trying to provide representation for {obj.GetType ()}.",
					e);
				return null;
			}
		}

		ISerializableObject SKImageToRepresentationImage (SKImage image)
			=> ImageFromSKData (image.Encode (), image.Width, image.Height);

		ISerializableObject ImageFromSKData (SKData data, int width, int height)
		{
			byte [] pngData;
			using (var ms = new MemoryStream ()) {
				data.SaveTo (ms);
				pngData = ms.ToArray ();
			}

			return new Image (ImageFormat.Png, pngData, width, height);
		}
	}
}
