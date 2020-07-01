using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WebAssembly
{
	[Obfuscation(Feature = "renaming", Exclude = true)]
	internal sealed class Runtime
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string InvokeJS(string js, out int exceptionResult);

		internal static string InvokeJS(string js)
		{
			var r = InvokeJS(js, out var exceptionResult);
			if (exceptionResult != 0)
			{
				Console.Error.WriteLine($"Error #{exceptionResult} \"{r}\" executing javascript: \"{js}\"");
			}
			return r;
		}
	}
}
