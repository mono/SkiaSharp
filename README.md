# SkiaSharp

Support for the Skia Library, to build, you want to get Skia from skia.org, it has instructions
on how to build for your platform.   

I am testing on OSX with a dylib created from the static objects, but there are now instructions
on creating a dynamic library for Skia here:

https://github.com/xamarin/skia/blob/master/experimental/c-api-example/c.md

If you want to use OSX and create the library like I did, just run the dylib script in this
directory and place the resulting library in /tmp/libskia.dylib

