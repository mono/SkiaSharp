using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	public class Language : NativeObject
	{
		public static Language Default => new Language (HarfBuzzApi.hb_language_get_default ());

		internal Language (IntPtr handle)
			: base (handle) { }

		public Language (CultureInfo culture) : this (culture.TwoLetterISOLanguageName) { }

		public Language (string name) : base (IntPtr.Zero)
		{
			var bytes = Encoding.UTF8.GetBytes (name);
			Handle = HarfBuzzApi.hb_language_from_string (bytes, bytes.Length);
		}

		public string Name => Marshal.PtrToStringAnsi (HarfBuzzApi.hb_language_to_string (Handle));

		public override string ToString () => Name;
	}
}
