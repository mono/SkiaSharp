using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SkiaSharp;

namespace WebAssembly
{
	internal sealed class Runtime
	{
		/// <summary>
		/// Mono specific internal call.
		/// </summary>
		[MethodImpl (MethodImplOptions.InternalCall)]
		private static extern string InvokeJS (string str, out int exceptional_result);

		// Disable inlining to avoid the interpreter to evaluate an internal call that may not be available
		[MethodImpl (MethodImplOptions.NoInlining)]
		private static string MonoInvokeJS (string str, out int exceptionResult) => InvokeJS (str, out exceptionResult);

		// Disable inlining to avoid the interpreter to evaluate an internal call that may not be available
		[MethodImpl (MethodImplOptions.NoInlining)]
		private static string NetCoreInvokeJS (string str, out int exceptionResult)
			=> Interop.Runtime.InvokeJS (str, out exceptionResult);

		/// <summary>
		/// Invokes Javascript code in the hosting environment
		/// </summary>
		internal static string InvokeJS (string str)
		{
			var r = IsNetCore
			? NetCoreInvokeJS (str, out var exceptionResult)
			: MonoInvokeJS (str, out exceptionResult);

			if (exceptionResult != 0) {
				Console.Error.WriteLine ($"Error #{exceptionResult} \"{r}\" executing javascript: \"{str}\"");
			}
			return r;
		}

		internal static bool IsNetCore
#if NET5_0
			 => true;
#else
			 => Type.GetType ("System.Runtime.Loader.AssemblyLoadContext") != null;
#endif
	}
}

internal sealed class Interop
{
	internal sealed class Runtime
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern string InvokeJS (string str, out int exceptional_result);
	}
}


namespace SkiaSharp
{
	public static class WebAssemblyRuntime
	{
		private static Dictionary<string, IntPtr> MethodMap = new Dictionary<string, IntPtr> ();

		public static bool IsWebAssembly { get; }
			= RuntimeInformation.IsOSPlatform (OSPlatform.Create ("WEBASSEMBLY"));

		private static IntPtr GetMethodId (string methodName)
		{
			if (!MethodMap.TryGetValue (methodName, out var methodId)) {
				MethodMap[methodName] = methodId = WebAssembly.JSInterop.InternalCalls.InvokeJSUnmarshalled (out var e, methodName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			}

			return methodId;
		}

		 
		/// <summary>
		/// Invoke a Javascript method using unmarshaled conversion.
		/// </summary>
		/// <param name="functionIdentifier">A function identifier name</param>
		public static IntPtr InvokeJSUnmarshalled (string functionIdentifier, IntPtr arg0)
		{
			var methodId = GetMethodId (functionIdentifier);

			var res = WebAssembly.JSInterop.InternalCalls.InvokeJSUnmarshalled (out var exception, null, methodId, arg0, IntPtr.Zero);

			if (exception != null) {
				throw new Exception (exception);
			}

			return res;
		}

		/// <summary>
		/// Invoke a Javascript method using unmarshaled conversion.
		/// </summary>
		/// <param name="functionIdentifier">A function identifier name</param>
		public static IntPtr InvokeJSUnmarshalled (string functionIdentifier, IntPtr arg0, IntPtr arg1)
		{
			var methodId = GetMethodId (functionIdentifier);

			var res = WebAssembly.JSInterop.InternalCalls.InvokeJSUnmarshalled (out var exception, null, methodId, arg0, arg1);

			if (exception != null) {
				throw new Exception (exception);
			}

			return res;
		}

		/// <summary>
		/// Provides an override for javascript invokes.
		/// </summary>
		public static Func<string, string> InvokeJSOverride;

		public static string InvokeJS (string str)
		{
			string result;

			if (InvokeJSOverride == null) {
				result = WebAssembly.Runtime.InvokeJS (str);
			} else {
				result = InvokeJSOverride (str);
			}

			if (result == null) {
				throw new InvalidOperationException ("The invoked Javascript method did not return a value (" + str + ")");
			}

			return result;
		}
		 
		public static object GetObjectFromGcHandle (string intPtr)
		{
			var ptr = Marshal.StringToHGlobalAuto (intPtr);
			var handle = GCHandle.FromIntPtr (ptr);
			return handle.IsAllocated ? handle.Target : null;
		}

		[Pure] 
		public static string EscapeJs (string s)
		{
			if (s == null) {
				return "";
			}

			bool NeedsEscape (string s2)
			{
				for (int i = 0; i < s2.Length; i++) {
					var c = s2[i];

					if (
						c > 255
						|| c < 32
						|| c == '\\'
						|| c == '"'
						|| c == '\r'
						|| c == '\n'
						|| c == '\t'
					) {
						return true;
					}
				}

				return false; 
			} 

			if (NeedsEscape (s)) {
				var r = new StringBuilder (s.Length);

				foreach (var c in s) {
					switch (c) {
						case '\\':
							r.Append ("\\\\");
							continue; 
						case '"':
							r.Append ("\\\"");
							continue;
						case '\r':
							continue;
						case '\n':
							r.Append ("\\n");
							continue;
						case '\t':
							r.Append ("\\t");
							continue;
					}

					if (c < 32) {
						continue; // not displayable
					}

					if (c <= 255) {
						r.Append (c);
					} else {
						r.Append ("\\u");
						r.Append (((ushort)c).ToString ("X4"));
					}
				}

				return r.ToString ();
			} else {
				return s;
			}
		}
	} 
}
