using System;
using System.Globalization;
using System.Runtime.InteropServices;

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
			Handle = HarfBuzzApi.hb_language_from_string (name, -1);
			Name = Marshal.PtrToStringAnsi (HarfBuzzApi.hb_language_to_string (Handle));
		}

		public string Name { get; }

		public override string ToString () => Name;

		protected bool Equals (Language other) => string.Equals (Name, other.Name);

		public override int GetHashCode () => (Name != null ? Name.GetHashCode () : 0);
	}
}
