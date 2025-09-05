#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// To be added.
	/// </summary>
	/// <remarks>
	/// To be added.
	/// </remarks>
	public unsafe class UnicodeFunctions : NativeObject
	{
		private static readonly Lazy<UnicodeFunctions> defaultFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ()));

		private static readonly Lazy<UnicodeFunctions> emptyFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ()));

		/// <summary>
		/// To be added.
		/// </summary>
		/// <value>To be added.</value>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public static UnicodeFunctions Default => defaultFunctions.Value;

		public static UnicodeFunctions Empty => emptyFunctions.Value;

		internal UnicodeFunctions (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="parent">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public UnicodeFunctions (UnicodeFunctions parent) : base (IntPtr.Zero)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof (parent));
			if (parent.Handle == IntPtr.Zero)
				throw new ArgumentException (nameof (parent.Handle));

			Parent = parent;
			Handle = HarfBuzzApi.hb_unicode_funcs_create (parent.Handle);
		}

		public UnicodeFunctions Parent { get; }

		public bool IsImmutable => HarfBuzzApi.hb_unicode_funcs_is_immutable (Handle);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void MakeImmutable () => HarfBuzzApi.hb_unicode_funcs_make_immutable (Handle);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public UnicodeCombiningClass GetCombiningClass (int unicode) => GetCombiningClass ((uint)unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public UnicodeCombiningClass GetCombiningClass (uint unicode) =>
			HarfBuzzApi.hb_unicode_combining_class (Handle, unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public UnicodeGeneralCategory GetGeneralCategory (int unicode) => GetGeneralCategory ((uint)unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public UnicodeGeneralCategory GetGeneralCategory (uint unicode) =>
			HarfBuzzApi.hb_unicode_general_category (Handle, unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public int GetMirroring (int unicode) => (int)GetMirroring ((uint)unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public uint GetMirroring (uint unicode) => HarfBuzzApi.hb_unicode_mirroring (Handle, unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public Script GetScript (int unicode) => GetScript ((uint)unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="unicode">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public Script GetScript (uint unicode) => HarfBuzzApi.hb_unicode_script (Handle, unicode);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="a">To be added.</param>
		/// <param name="b">To be added.</param>
		/// <param name="ab">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public bool TryCompose (int a, int b, out int ab)
		{
			var result = TryCompose ((uint)a, (uint)b, out var composed);

			ab = (int)composed;

			return result;
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="a">To be added.</param>
		/// <param name="b">To be added.</param>
		/// <param name="ab">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public bool TryCompose (uint a, uint b, out uint ab)
		{
			fixed (uint* abPtr = &ab) {
				return HarfBuzzApi.hb_unicode_compose (Handle, a, b, abPtr);
			}
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="ab">To be added.</param>
		/// <param name="a">To be added.</param>
		/// <param name="b">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public bool TryDecompose (int ab, out int a, out int b)
		{
			var result = TryDecompose ((uint)ab, out var decomposedA, out var decomposedB);

			a = (int)decomposedA;

			b = (int)decomposedB;

			return result;
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="ab">To be added.</param>
		/// <param name="a">To be added.</param>
		/// <param name="b">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public bool TryDecompose (uint ab, out uint a, out uint b)
		{
			fixed (uint* aPtr = &a)
			fixed (uint* bPtr = &b) {
				return HarfBuzzApi.hb_unicode_decompose (Handle, ab, aPtr, bPtr);
			}
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetCombiningClassDelegate (CombiningClassDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_combining_class_func (
				Handle, DelegateProxies.UnicodeCombiningClassProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetGeneralCategoryDelegate (GeneralCategoryDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_general_category_func (
				Handle, DelegateProxies.UnicodeGeneralCategoryProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetMirroringDelegate (MirroringDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_mirroring_func (
				Handle, DelegateProxies.UnicodeMirroringProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetScriptDelegate (ScriptDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_script_func (
				Handle, DelegateProxies.UnicodeScriptProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetComposeDelegate (ComposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_compose_func (
				Handle, DelegateProxies.UnicodeComposeProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="del">To be added.</param>
		/// <param name="destroy">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public void SetDecomposeDelegate (DecomposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_decompose_func (
				Handle, DelegateProxies.UnicodeDecomposeProxy, (void*)ctx, DelegateProxies.DestroyProxyForMulti);
		}

		private void VerifyParameters (Delegate del)
		{
			_ = del ?? throw new ArgumentNullException (nameof (del));

			if (IsImmutable)
				throw new InvalidOperationException ($"{nameof (UnicodeFunctions)} is immutable and can't be changed.");
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="disposing">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>
		/// To be added.
		/// </remarks>
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
