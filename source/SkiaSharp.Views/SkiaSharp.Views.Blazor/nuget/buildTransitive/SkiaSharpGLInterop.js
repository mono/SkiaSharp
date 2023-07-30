// Workaround for https://github.com/dotnet/runtime/issues/76077
// Special thanks to the Avalonia UI team

var SkiaSharpGLInterop = {
	$SkiaSharpLibrary: {
		internal_func: function () {
		}
	},
	InterceptGLObject: function () {
		globalThis.SkiaSharpGL = GL
	}
}

autoAddDeps(SkiaSharpGLInterop, '$SkiaSharpLibrary')
mergeInto(LibraryManager.library, SkiaSharpGLInterop)
