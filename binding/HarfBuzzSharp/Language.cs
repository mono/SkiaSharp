#nullable disable

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public unsafe class Language : NativeObject
	{
		private static readonly Lazy<Language> defaultLanguage =
			new Lazy<Language> (() => new StaticLanguage (HarfBuzzApi.hb_language_get_default ()));

		public static Language Default => defaultLanguage.Value;

		internal Language (IntPtr handle)
			: base (handle)
		{
		}

		public Language (CultureInfo culture)
			: this (culture.TwoLetterISOLanguageName)
		{
		}

		public Language (string name)
			: base (IntPtr.Zero)
		{
			Handle = HarfBuzzApi.hb_language_from_string (name, -1);
			Name = Marshal.PtrToStringAnsi ((IntPtr)HarfBuzzApi.hb_language_to_string (Handle));
		}

		public string Name { get; }

		public override string ToString () => Name;

		public override bool Equals (object obj) =>
			obj is Language language && Handle == language.Handle;

		public override int GetHashCode () => Name != null ? Name.GetHashCode () : 0;

		private class StaticLanguage : Language
		{
			public StaticLanguage (IntPtr handle)
				: base (handle)
			{
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
