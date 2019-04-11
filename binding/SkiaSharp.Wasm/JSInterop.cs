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
	[Obfuscation (Feature = "renaming", Exclude = true)]
	internal sealed class Runtime
	{
		[MethodImpl (MethodImplOptions.InternalCall)]
		private static extern string InvokeJS (string str, out int exceptional_result);

		internal static string InvokeJS (string str)
		{
			var r = InvokeJS (str, out var exceptionResult);
			if (exceptionResult != 0) {
				Console.Error.WriteLine ($"Error #{exceptionResult} \"{r}\" executing javascript: \"{str}\"");
			}
			return r;
		}
	}

	namespace JSInterop
	{
		internal static class InternalCalls
		{ 
			// Matches this signature:
			// https://github.com/mono/mono/blob/f24d652d567c4611f9b4e3095be4e2a1a2ab23a4/sdks/wasm/driver.c#L21
			[MethodImpl (MethodImplOptions.InternalCall)]
			public static extern IntPtr InvokeJSUnmarshalled (out string exception, string functionIdentifier, IntPtr arg0, IntPtr arg1, IntPtr arg2);
		}
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
	  
	public static class TSInteropMarshaller
	{
		public const UnmanagedType LPUTF8Str = (UnmanagedType)48;

		public static IntPtr InvokeJS<TParam> (
			string methodName,
			TParam paramStruct,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = null
		)
		{
			Console.WriteLine($"InvokeJS for {memberName}/{typeof (TParam)}");

			var pParms = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (TParam)));

			try {
				Marshal.StructureToPtr (paramStruct, pParms, false);

				return WebAssemblyRuntime.InvokeJSUnmarshalled (methodName, pParms);
			} catch (Exception e) {
				Console.WriteLine ($"Failed InvokeJS for {memberName}/{typeof (TParam)}: {e}");
				throw;
			} finally {
				Marshal.DestroyStructure (pParms, typeof (TParam));
				Marshal.FreeHGlobal (pParms);
			}
		}

		public static IntPtr InvokeJS<TParam, TRet> (
			string methodName,
			TParam paramStruct,
			out TRet returnValue,
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = null
		)
		{  
			Console.WriteLine ($"InvokeJS for {memberName}/{typeof (TParam)}/{typeof (TRet)}");
			 
			var pParms = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (TParam)));
			var pReturnValue = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (TRet)));

			returnValue = default;

			try {
				Marshal.StructureToPtr (paramStruct, pParms, false);
				Marshal.StructureToPtr (returnValue, pReturnValue, false);

				var ret = WebAssemblyRuntime.InvokeJSUnmarshalled (methodName, pParms, pReturnValue);

				returnValue = (TRet)Marshal.PtrToStructure (pReturnValue, typeof (TRet));

				return ret;
			} catch (Exception e) {
					Console.WriteLine ($"Failed InvokeJS for {memberName}/{typeof (TParam)}: {e}");
				throw;
			} finally {
				Marshal.DestroyStructure (pParms, typeof (TParam));
				Marshal.FreeHGlobal (pParms);

				Marshal.DestroyStructure (pReturnValue, typeof (TRet));
				Marshal.FreeHGlobal (pReturnValue);
			}
		}
	}

	/// <summary>
	/// Marks a struct as an interop message for the <see cref="TSBindingsGenerator"/> TypeScript generator.
	/// </summary>
	[AttributeUsage (AttributeTargets.Struct, AllowMultiple = false)]
	public class TSInteropMessageAttribute : Attribute
	{
	}
}
