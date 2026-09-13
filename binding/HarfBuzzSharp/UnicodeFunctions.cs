#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Represents a collection of callback functions used by HarfBuzz for Unicode character property lookups such as script, general category, and combining class.</summary>
	/// <remarks />
	public unsafe class UnicodeFunctions : NativeObject
	{
		private static readonly Lazy<UnicodeFunctions> defaultFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ()));

		private static readonly Lazy<UnicodeFunctions> emptyFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ()));

		/// <summary>Gets the default <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance with built-in Unicode support.</summary>
		/// <value>The default Unicode functions instance.</value>
		/// <remarks />
		public static UnicodeFunctions Default => defaultFunctions.Value;

		/// <summary>Gets a reference to the empty <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance with no callbacks set.</summary>
		/// <value>The empty Unicode functions instance.</value>
		/// <remarks />
		public static UnicodeFunctions Empty => emptyFunctions.Value;

		internal UnicodeFunctions (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> class that inherits from the specified parent.</summary>
		/// <param name="parent">The parent Unicode functions to inherit from for any unset callbacks, or <see langword="null" />.</param>
		/// <remarks />
		public UnicodeFunctions (UnicodeFunctions parent) : base (IntPtr.Zero)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof (parent));
			if (parent.Handle == IntPtr.Zero)
				throw new ArgumentException (nameof (parent.Handle));

			Parent = parent;
			Handle = HarfBuzzApi.hb_unicode_funcs_create (parent.Handle);
		}

		/// <summary>Gets the parent Unicode functions that this instance inherits from.</summary>
		/// <value>The parent <see cref="T:HarfBuzzSharp.UnicodeFunctions" />, or <see langword="null" /> if there is no parent.</value>
		/// <remarks />
		public UnicodeFunctions Parent { get; }

		/// <summary>Gets a value indicating whether this Unicode functions instance is immutable.</summary>
		/// <value><see langword="true" /> if the instance is immutable; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsImmutable {
			get {
				var r = HarfBuzzApi.hb_unicode_funcs_is_immutable (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Makes this Unicode functions instance immutable, preventing further modifications.</summary>
		/// <remarks />
		public void MakeImmutable ()
		{
			HarfBuzzApi.hb_unicode_funcs_make_immutable (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Gets the canonical combining class of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeCombiningClass" /> for the code point.</returns>
		/// <remarks />
		public UnicodeCombiningClass GetCombiningClass (int unicode) => GetCombiningClass ((uint)unicode);

		/// <summary>Gets the canonical combining class of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeCombiningClass" /> for the code point.</returns>
		/// <remarks />
		public UnicodeCombiningClass GetCombiningClass (uint unicode)
		{
			var r = HarfBuzzApi.hb_unicode_combining_class (Handle, unicode);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets the general category of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeGeneralCategory" /> for the code point.</returns>
		/// <remarks />
		public UnicodeGeneralCategory GetGeneralCategory (int unicode) => GetGeneralCategory ((uint)unicode);

		/// <summary>Gets the general category of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeGeneralCategory" /> for the code point.</returns>
		/// <remarks />
		public UnicodeGeneralCategory GetGeneralCategory (uint unicode)
		{
			var r = HarfBuzzApi.hb_unicode_general_category (Handle, unicode);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets the mirrored code point for bidirectional text rendering.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The mirrored Unicode code point, or the original code point if no mirroring is defined.</returns>
		/// <remarks />
		public int GetMirroring (int unicode) => (int)GetMirroring ((uint)unicode);

		/// <summary>Gets the mirrored code point for bidirectional text rendering.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The mirrored Unicode code point, or the original code point if no mirroring is defined.</returns>
		/// <remarks />
		public uint GetMirroring (uint unicode)
		{
			var r = HarfBuzzApi.hb_unicode_mirroring (Handle, unicode);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets the script of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.Script" /> for the code point.</returns>
		/// <remarks />
		public Script GetScript (int unicode) => GetScript ((uint)unicode);

		/// <summary>Gets the script of the specified Unicode code point.</summary>
		/// <param name="unicode">The Unicode code point to query.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.Script" /> for the code point.</returns>
		/// <remarks />
		public Script GetScript (uint unicode)
		{
			var r = HarfBuzzApi.hb_unicode_script (Handle, unicode);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Attempts to compose two Unicode code points into a single code point.</summary>
		/// <param name="a">The first Unicode code point to compose.</param>
		/// <param name="b">The second Unicode code point to compose.</param>
		/// <param name="ab">When this method returns, contains the composed Unicode code point if successful.</param>
		/// <returns><see langword="true" /> if the code points were successfully composed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryCompose (int a, int b, out int ab)
		{
			var result = TryCompose ((uint)a, (uint)b, out var composed);

			ab = (int)composed;

			return result;
		}

		/// <summary>Attempts to compose two Unicode code points into a single code point.</summary>
		/// <param name="a">The first Unicode code point to compose.</param>
		/// <param name="b">The second Unicode code point to compose.</param>
		/// <param name="ab">When this method returns, contains the composed Unicode code point if successful.</param>
		/// <returns><see langword="true" /> if the code points were successfully composed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryCompose (uint a, uint b, out uint ab)
		{
			fixed (uint* abPtr = &ab) {
				var r = HarfBuzzApi.hb_unicode_compose (Handle, a, b, abPtr);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Attempts to decompose a Unicode code point into two code points.</summary>
		/// <param name="ab">The Unicode code point to decompose.</param>
		/// <param name="a">When this method returns, contains the first component of the decomposition if successful.</param>
		/// <param name="b">When this method returns, contains the second component of the decomposition if successful.</param>
		/// <returns><see langword="true" /> if the code point was successfully decomposed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryDecompose (int ab, out int a, out int b)
		{
			var result = TryDecompose ((uint)ab, out var decomposedA, out var decomposedB);

			a = (int)decomposedA;

			b = (int)decomposedB;

			return result;
		}

		/// <summary>Attempts to decompose a Unicode code point into two code points.</summary>
		/// <param name="ab">The Unicode code point to decompose.</param>
		/// <param name="a">When this method returns, contains the first component of the decomposition if successful.</param>
		/// <param name="b">When this method returns, contains the second component of the decomposition if successful.</param>
		/// <returns><see langword="true" /> if the code point was successfully decomposed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryDecompose (uint ab, out uint a, out uint b)
		{
			fixed (uint* aPtr = &a)
			fixed (uint* bPtr = &b) {
				var r = HarfBuzzApi.hb_unicode_decompose (Handle, ab, aPtr, bPtr);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Sets the callback for retrieving the canonical combining class of a Unicode code point.</summary>
		/// <param name="del">The delegate to set for retrieving combining classes.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetCombiningClassDelegate (CombiningClassDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_combining_class_func (
				Handle, DelegateProxies.UnicodeCombiningClassProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the callback for retrieving the general category of a Unicode code point.</summary>
		/// <param name="del">The delegate to set for retrieving general categories.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetGeneralCategoryDelegate (GeneralCategoryDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_general_category_func (
				Handle, DelegateProxies.UnicodeGeneralCategoryProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the callback for retrieving the mirrored glyph of a Unicode code point.</summary>
		/// <param name="del">The delegate to set for retrieving mirrored characters.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetMirroringDelegate (MirroringDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_mirroring_func (
				Handle, DelegateProxies.UnicodeMirroringProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the callback for retrieving the script of a Unicode code point.</summary>
		/// <param name="del">The delegate to set for retrieving scripts.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetScriptDelegate (ScriptDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_script_func (
				Handle, DelegateProxies.UnicodeScriptProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the callback for composing two Unicode code points into a single code point.</summary>
		/// <param name="del">The delegate to set for Unicode composition.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetComposeDelegate (ComposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_compose_func (
				Handle, DelegateProxies.UnicodeComposeProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		/// <summary>Sets the callback for decomposing a Unicode code point into two code points.</summary>
		/// <param name="del">The delegate to set for Unicode decomposition.</param>
		/// <param name="destroy">The delegate to call when the callback is replaced or destroyed, or <see langword="null" />.</param>
		/// <remarks />
		public void SetDecomposeDelegate (DecomposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_decompose_func (
				Handle, DelegateProxies.UnicodeDecomposeProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
			GC.KeepAlive (this);
		}

		private void VerifyParameters (Delegate del)
		{
			_ = del ?? throw new ArgumentNullException (nameof (del));

			if (IsImmutable)
				throw new InvalidOperationException ($"{nameof (UnicodeFunctions)} is immutable and can't be changed.");
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the unmanaged resources used.</summary>
		/// <remarks />
		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_unicode_funcs_destroy (Handle);
			}
		}

		private class StaticUnicodeFunctions : UnicodeFunctions
		{
			public StaticUnicodeFunctions (IntPtr handle)
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
