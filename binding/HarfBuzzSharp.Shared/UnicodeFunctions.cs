using System;

namespace HarfBuzzSharp
{
	public unsafe class UnicodeFunctions : NativeObject
	{
		private static readonly Lazy<UnicodeFunctions> defaultFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ()));

		private static readonly Lazy<UnicodeFunctions> emptyFunctions =
			new Lazy<UnicodeFunctions> (() => new StaticUnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ()));

		public static UnicodeFunctions Default => defaultFunctions.Value;

		public static UnicodeFunctions Empty => emptyFunctions.Value;

		internal UnicodeFunctions (IntPtr handle)
			: base (handle)
		{
		}

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

		public void MakeImmutable () => HarfBuzzApi.hb_unicode_funcs_make_immutable (Handle);

		public UnicodeCombiningClass GetCombiningClass (int unicode) => GetCombiningClass ((uint)unicode);

		public UnicodeCombiningClass GetCombiningClass (uint unicode) =>
			HarfBuzzApi.hb_unicode_combining_class (Handle, unicode);

		public UnicodeGeneralCategory GetGeneralCategory (int unicode) => GetGeneralCategory ((uint)unicode);

		public UnicodeGeneralCategory GetGeneralCategory (uint unicode) =>
			HarfBuzzApi.hb_unicode_general_category (Handle, unicode);

		public int GetMirroring (int unicode) => (int)GetMirroring ((uint)unicode);

		public uint GetMirroring (uint unicode) => HarfBuzzApi.hb_unicode_mirroring (Handle, unicode);

		public Script GetScript (int unicode) => GetScript ((uint)unicode);

		public Script GetScript (uint unicode) => HarfBuzzApi.hb_unicode_script (Handle, unicode);

		public bool TryCompose (int a, int b, out int ab)
		{
			var result = TryCompose ((uint)a, (uint)b, out var composed);

			ab = (int)composed;

			return result;
		}

		public bool TryCompose (uint a, uint b, out uint ab)
		{
			fixed (uint* abPtr = &ab) {
				return HarfBuzzApi.hb_unicode_compose (Handle, a, b, abPtr);
			}
		}

		public bool TryDecompose (int ab, out int a, out int b)
		{
			var result = TryDecompose ((uint)ab, out var decomposedA, out var decomposedB);

			a = (int)decomposedA;

			b = (int)decomposedB;

			return result;
		}

		public bool TryDecompose (uint ab, out uint a, out uint b)
		{
			fixed (uint* aPtr = &a)
			fixed (uint* bPtr = &b) {
				return HarfBuzzApi.hb_unicode_decompose (Handle, ab, aPtr, bPtr);
			}
		}

		public void SetCombiningClassDelegate (CombiningClassDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_combining_class_func (
				Handle, DelegateProxies.CombiningClassProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetGeneralCategoryDelegate (GeneralCategoryDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_general_category_func (
				Handle, DelegateProxies.GeneralCategoryProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetMirroringDelegate (MirroringDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_mirroring_func (
				Handle, DelegateProxies.MirroringProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetScriptDelegate (ScriptDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_script_func (
				Handle, DelegateProxies.ScriptProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetComposeDelegate (ComposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_compose_func (
				Handle, DelegateProxies.ComposeProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		public void SetDecomposeDelegate (DecomposeDelegate del, ReleaseDelegate destroy = null)
		{
			VerifyParameters (del);

			var ctx = DelegateProxies.CreateMultiUserData (del, destroy, this);
			HarfBuzzApi.hb_unicode_funcs_set_decompose_func (
				Handle, DelegateProxies.DecomposeProxy, (void*)ctx, DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		private void VerifyParameters (Delegate del)
		{
			_ = del ?? throw new ArgumentNullException (nameof (del));

			if (IsImmutable)
				throw new InvalidOperationException ($"{nameof (UnicodeFunctions)} is immutable and can't be changed.");
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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
