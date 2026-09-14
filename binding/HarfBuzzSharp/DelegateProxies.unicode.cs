#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>A callback delegate used to retrieve the canonical combining class of a Unicode code point.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="unicode">The Unicode code point to query.</param>
	/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeCombiningClass" /> for the specified code point.</returns>
	/// <remarks />
	public delegate UnicodeCombiningClass CombiningClassDelegate (UnicodeFunctions ufuncs, uint unicode);

	/// <summary>A callback delegate used to retrieve the general category of a Unicode code point.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="unicode">The Unicode code point to query.</param>
	/// <returns>The <see cref="T:HarfBuzzSharp.UnicodeGeneralCategory" /> for the specified code point.</returns>
	/// <remarks />
	public delegate UnicodeGeneralCategory GeneralCategoryDelegate (UnicodeFunctions ufuncs, uint unicode);

	/// <summary>A callback delegate used to retrieve the mirrored glyph of a Unicode code point for bidirectional text rendering.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="unicode">The Unicode code point to query.</param>
	/// <returns>The mirrored Unicode code point, or the original code point if no mirroring is defined.</returns>
	/// <remarks />
	public delegate uint MirroringDelegate (UnicodeFunctions ufuncs, uint unicode);

	/// <summary>A callback delegate used to retrieve the script of a Unicode code point.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="unicode">The Unicode code point to query.</param>
	/// <returns>The <see cref="T:HarfBuzzSharp.Script" /> for the specified code point.</returns>
	/// <remarks />
	public delegate Script ScriptDelegate (UnicodeFunctions ufuncs, uint unicode);

	/// <summary>A callback delegate used to compose two Unicode code points into a single code point using Unicode canonical composition.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="a">The first Unicode code point to compose.</param>
	/// <param name="b">The second Unicode code point to compose.</param>
	/// <param name="ab">When this method returns, contains the composed Unicode code point.</param>
	/// <returns><see langword="true" /> if the code points were successfully composed; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool ComposeDelegate (UnicodeFunctions ufuncs, uint a, uint b, out uint ab);

	/// <summary>A callback delegate used to decompose a Unicode code point into two code points using Unicode canonical decomposition.</summary>
	/// <param name="ufuncs">The <see cref="T:HarfBuzzSharp.UnicodeFunctions" /> instance invoking this callback.</param>
	/// <param name="ab">The Unicode code point to decompose.</param>
	/// <param name="a">When this method returns, contains the first component of the decomposition.</param>
	/// <param name="b">When this method returns, contains the second component of the decomposition.</param>
	/// <returns><see langword="true" /> if the code point was successfully decomposed; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public delegate bool DecomposeDelegate (UnicodeFunctions ufuncs, uint ab, out uint a, out uint b);

	internal static unsafe partial class DelegateProxies
	{
		private static partial int UnicodeCombiningClassProxyImplementation (IntPtr ufuncs, uint unicode, void* user_data)
		{
			GetMultiUserData<CombiningClassDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

		private static partial int UnicodeGeneralCategoryProxyImplementation (IntPtr ufuncs, uint unicode, void* user_data)
		{
			GetMultiUserData<GeneralCategoryDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

		private static partial uint UnicodeMirroringProxyImplementation (IntPtr ufuncs, uint unicode, void* user_data)
		{
			GetMultiUserData<MirroringDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

		private static partial uint UnicodeScriptProxyImplementation (IntPtr ufuncs, uint unicode, void* user_data)
		{
			GetMultiUserData<ScriptDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

		private static partial bool UnicodeComposeProxyImplementation (IntPtr ufuncs, uint a, uint b, uint* ab, void* user_data)
		{
			GetMultiUserData<ComposeDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			var result = del.Invoke (functions, a, b, out var abManaged);
			if (ab != null)
				*ab = abManaged;
			return result;
		}

		private static partial bool UnicodeDecomposeProxyImplementation (IntPtr ufuncs, uint ab, uint* a, uint* b, void* user_data)
		{
			GetMultiUserData<DecomposeDelegate, UnicodeFunctions> ((IntPtr)user_data, out var del, out var functions, out _);
			var result = del.Invoke (functions, ab, out var aManaged, out var bManaged);
			if (a != null)
				*a = aManaged;
			if (b != null)
				*b = bManaged;
			return result;
		}
	}
}
