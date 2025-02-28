// Workaround for https://github.com/dotnet/runtime/issues/76077
// Special thanks to the Avalonia UI team

var SkiaSharpInterop = {
	$SkiaSharpLibrary: {
		internal_func: function () {
		}
	},
    InterceptBrowserObjects: function () {
		globalThis.SkiaSharpGL = GL
        globalThis.SkiaSharpModule = Module
    }
}

autoAddDeps(SkiaSharpInterop, '$SkiaSharpLibrary')
mergeInto(LibraryManager.library, SkiaSharpInterop)
