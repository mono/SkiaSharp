using System;
using System.Globalization;
using System.Reflection;

namespace SkiaSharp
{
	internal partial class SkiaApi
	{
#if __TVOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __WATCHOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __IOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __ANDROID__
		private const string SKIA = "libSkiaSharp.so";
#elif __MACOS__
		private const string SKIA = "libSkiaSharp.dylib";
#elif WINDOWS_UWP
		private const string SKIA = "libSkiaSharp.dll";
#elif __TIZEN__
		private const string SKIA = "libSkiaSharp.so";
#else
		private const string SKIA = "libSkiaSharp";
#endif

#if USE_DELEGATES
		private static readonly Lazy<IntPtr> libSkiaSharpHandle =
			new Lazy<IntPtr> (() => LibraryLoader.LoadLocalLibrary<SkiaApi> (SKIA));

		private static T GetSymbol<T> (string name) where T : Delegate =>
			LibraryLoader.GetSymbolDelegate<T> (libSkiaSharpHandle.Value, name);
#endif

#if __WASM__
		static SkiaApi ()
		{
			const string js = @"
(window || global).SkiaSharp_SkiaApi = class SkiaApi {
  static createJsMethod(monoMethod) {
    return (...args) => {
      return monoMethod.apply(undefined, args);
    };
  }
  static bindMembers(type, members) {
    let ptrs = [];
    for (let member in members) {
      let monoMethod = BINDING.bind_static_method(`${type}:${member}`);
      let jsMethod = SkiaApi.createJsMethod(monoMethod);
      let wasmMethod = Module.addFunction(jsMethod, members[member]);
      ptrs.push(wasmMethod);
    }
    return ptrs;
  }
};";
			WebAssembly.Runtime.InvokeJS (js);
		}

		internal static IntPtr[] BindWasmMembers<T> ((string name, string signature)[] members) =>
			BindWasmMembers (typeof (T), members);

		internal static IntPtr[] BindWasmMembers (Type type, (string name, string signature)[] members)
		{
#if DEBUG
			foreach (var (name, signature) in members) {
				var m = type.GetMethod (name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (m == null)
					throw new ArgumentException ($"Unable to find static method {type.Name}.{name}.");
				var p = m.GetParameters ();
				if (signature.Length != p.Length + 1)
					throw new ArgumentException ($"Parameters do not match {signature} in {type.Name}.{name}.");
			}
#endif

			var expected = members.Length;

			// build js
			var js = $"SkiaSharp_SkiaApi.bindMembers('[{type.Assembly.GetName ().Name}] {type.FullName}', {{";
			foreach (var (name, signature) in members) {
				js += $"  '{name}': '{signature}',";
			}
			js += "});";

			// run js
			var ret = WebAssembly.Runtime.InvokeJS (js);
			var split = ret.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (split.Length != expected)
				throw new InvalidOperationException ($"Mismatch when binding 'SkiaSharp.SKDrawable' members. Returned {split.Length}, expected {expected}.");

			// get IntPtrs
			var funcs = new IntPtr[expected];
			for (var i = 0; i < expected; i++) {
				funcs[i] = (IntPtr)int.Parse (split[i], CultureInfo.InvariantCulture);
			}

			return funcs;
		}
#endif
	}
}
