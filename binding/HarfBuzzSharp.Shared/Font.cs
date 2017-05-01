using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public class Font : NativeObject
	{
		internal Font(IntPtr handle)
			: base(handle)
		{
		}

		public Font(Face face)
			: this(IntPtr.Zero)
		{
			if (face == null)
			{
				throw new ArgumentNullException(nameof(face));
			}

			Handle = HarfBuzzApi.hb_font_create(face.Handle);
		}

		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero)
			{
				HarfBuzzApi.hb_font_destroy(Handle);
			}

			base.Dispose(disposing);
		}

		public void SetScale(int xScale, int yScale) => HarfBuzzApi.hb_font_set_scale(Handle, xScale, yScale);

		public void GetScale(out int xScale, out int yScale) => HarfBuzzApi.hb_font_get_scale(Handle, out xScale, out yScale);

		public void SetFunctionsOpenType() => HarfBuzzApi.hb_ot_font_set_funcs(Handle);

		public void Shape(Buffer buffer, params Feature[] features)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (features == null || features.Length == 0)
			{
				HarfBuzzApi.hb_shape(Handle, buffer.Handle, IntPtr.Zero, 0);
			}
			else
			{
				var ptr = StructureArrayToPtr(features);
				HarfBuzzApi.hb_shape(Handle, buffer.Handle, ptr, (uint)features.Length);
				Marshal.FreeCoTaskMem(ptr);
			}
		}
	}
}
