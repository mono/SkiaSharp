# SkiaSharp.Views.Graphite

Native `SKGraphiteView` containers for iOS, Mac Catalyst and compatible Android
devices. Private presenters currently use native Metal on Apple and Vulkan 1.1
on Android API 24 or later. Windows Graphite presentation is deferred.

This unsigned integration package isolates Android's unsigned Silk.NET.Vulkan
dependency without changing existing signed SkiaSharp.Views assemblies.
MAUI applications normally install `SkiaSharp.Views.Maui.Controls`, which brings
in this dependency only for supported platforms.

Paint callbacks borrow the frame surface and typed context. Use the view's
recorder helper for caller-owned producer work; stop workers and dispose their
resources during `ContextDestroying`. Do not submit or dispose the borrowed
context. There is no silent engine or software fallback.

See [GPU engine views](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/gpu-engine-views.md)
for lifecycle, threading, image caching and failure semantics.
