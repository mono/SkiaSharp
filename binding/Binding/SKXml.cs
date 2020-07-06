using System;

namespace SkiaSharp
{
	public abstract class SKXmlWriter : SKObject
	{
		private protected SKXmlWriter (IntPtr handle, bool owns = true, bool registerHandle = true)
			: base (handle, owns, registerHandle)
		{
		}
	}

	public class SKXmlStreamWriter : SKXmlWriter
	{
		public SKXmlStreamWriter (SKWStream stream)
			: base (IntPtr.Zero, true, false)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			RegisterHandle (SkiaApi.sk_xmlstreamwriter_new (stream.Handle));
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_xmlstreamwriter_delete (Handle);
	}
}
