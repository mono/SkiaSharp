#nullable disable

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>
	/// To be added.
	/// </summary>
	/// <remarks>To be added.</remarks>
	public unsafe class Language : NativeObject
	{
		private static readonly Lazy<Language> defaultLanguage =
			new Lazy<Language> (() => new StaticLanguage (HarfBuzzApi.hb_language_get_default ()));

		/// <summary>
		/// To be added.
		/// </summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public static Language Default => defaultLanguage.Value;

		internal Language (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="culture">To be added.</param>
		/// <remarks>To be added.</remarks>
		public Language (CultureInfo culture)
			: this (culture.TwoLetterISOLanguageName)
		{
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="name">To be added.</param>
		/// <remarks>To be added.</remarks>
		public Language (string name)
			: base (IntPtr.Zero)
		{
			Handle = HarfBuzzApi.hb_language_from_string (name, -1);
			Name = Marshal.PtrToStringAnsi ((IntPtr)HarfBuzzApi.hb_language_to_string (Handle));
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public string Name { get; }

		/// <summary>
		/// To be added.
		/// </summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public override string ToString () => Name;

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="obj">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public override bool Equals (object obj) =>
			obj is Language language && Handle == language.Handle;

		/// <summary>
		/// To be added.
		/// </summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
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
