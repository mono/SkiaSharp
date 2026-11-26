#nullable disable

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>Represents a language tag used for text shaping.</summary>
	/// <remarks>Language tags are used to identify the language of text being shaped, which affects how certain characters are rendered.</remarks>
	public unsafe class Language : NativeObject
	{
		private static readonly Lazy<Language> defaultLanguage =
			new Lazy<Language> (() => new StaticLanguage (HarfBuzzApi.hb_language_get_default ()));

		/// <summary>Gets the default language based on the current locale.</summary>
		/// <value>The default language.</value>
		/// <remarks />
		public static Language Default => defaultLanguage.Value;

		internal Language (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Language" /> class from a <see cref="T:System.Globalization.CultureInfo" />.</summary>
		/// <param name="culture">The culture to create the language from.</param>
		/// <remarks />
		public Language (CultureInfo culture)
			: this (culture.TwoLetterISOLanguageName)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Language" /> class from a BCP 47 language tag string.</summary>
		/// <param name="name">The BCP 47 language tag string (for example, "en" or "en-US").</param>
		/// <remarks />
		public Language (string name)
			: base (IntPtr.Zero)
		{
			Handle = HarfBuzzApi.hb_language_from_string (name, -1);
			Name = Marshal.PtrToStringAnsi ((IntPtr)HarfBuzzApi.hb_language_to_string (Handle));
		}

		/// <summary>Gets the BCP 47 language tag string representation.</summary>
		/// <value>The language tag string.</value>
		/// <remarks />
		public string Name { get; }

		/// <summary>Returns a string representation of the language.</summary>
		/// <returns>The BCP 47 language tag string.</returns>
		/// <remarks />
		public override string ToString () => Name;

		/// <summary>Determines whether the specified object is equal to the current language.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override bool Equals (object obj) =>
			obj is Language language && Handle == language.Handle;

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current object.</returns>
		/// <remarks />
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
