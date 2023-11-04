using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly;

internal sealed partial class Window
{
	[JSImport("globalThis.window.eval")]
	internal static partial void Eval([JSMarshalAs<JSType.String>] string message);
}
