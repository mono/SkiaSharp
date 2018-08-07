using System;

namespace SkiaSharp
{
	public abstract class SKXmlWriter : SKObject
	{
		internal SKXmlWriter (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
	}

	public class SKXmlStreamWriter : SKXmlWriter
	{
		[Preserve]
		internal SKXmlStreamWriter (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public SKXmlStreamWriter (SKWStream stream)
			: this (IntPtr.Zero, true)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			Handle = SkiaApi.sk_xmlstreamwriter_new (stream.Handle);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_xmlstreamwriter_delete (Handle);
			}

			base.Dispose (disposing);
		}
	}
}
