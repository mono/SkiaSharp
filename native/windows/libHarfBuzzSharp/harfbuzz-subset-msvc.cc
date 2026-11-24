// MSVC translation unit for libHarfBuzzSharp.
//
// BinSkim rule BA2007 (EnableCriticalCompilerWarnings) fails, at error severity, any
// binary whose recorded compiler command line disables C4244 or C4267 -- the
// "possible loss of data" truncation warnings its policy requires to stay enabled.
// This project used to pass /wd4244 /wd4267 in every ItemDefinitionGroup, so every
// consumer of HarfBuzzSharp.NativeAssets.Win32 inherited a BA2007 failure. See
// https://github.com/mono/SkiaSharp/issues/5108.
//
// Why suppress rather than fix: the warnings are entirely in vendored upstream
// HarfBuzz (~1200 at /W3 -- harfbuzz-subset.cc is an amalgamation of 73 upstream
// sources). Upstream treats them as intentional and suppresses both in its own
// official build systems, so they will not disappear on a future roll:
//
//   CMakeLists.txt  add_compile_options(/wd4244 /wd4267) # lossy type conversion
//   meson.build     '/wd4244' -- listed under "Ignore several spurious warnings for
//                   things HarfBuzz does very commonly ... Only add warnings here if
//                   you are sure they're spurious"
//
// Checked against HarfBuzz 14.2.1, where CMakeLists.txt blames to
// harfbuzz/harfbuzz#5642 ("MSVC: Sync compiler options between Meson and CMake",
// merged 2025-11) -- so this is a maintained decision rather than an oversight.
//
// So the suppression has to stay; the only question is where it lives. In source it
// keeps the compiler command line clean -- which is the only thing BA2007 inspects --
// and it narrows the blast radius to this one include, so any first-party code added
// to this project later still gets the warnings. If upstream ever moves these pragmas
// into harfbuzz-subset.cc itself, this wrapper can be deleted outright.

#pragma warning(push)
#pragma warning(disable : 4244) // conversion from 'x' to 'y', possible loss of data
#pragma warning(disable : 4267) // conversion from 'size_t' to 'y', possible loss of data

#include "../../../externals/skia/third_party/externals/harfbuzz/src/harfbuzz-subset.cc"

#pragma warning(pop)
