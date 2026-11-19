#pragma once

#if defined(SKIA_SHARP_VIEWS_UWP_NATIVE) && defined(SKIA_SHARP_VIEWS_WINUI_NATIVE)
#error Only one WinRT native platform namespace can be selected.
#elif defined(SKIA_SHARP_VIEWS_UWP_NATIVE)
#define SKIA_SHARP_VIEWS_NATIVE_NAMESPACE winrt::SkiaSharp::Views::UWP::Native
#elif defined(SKIA_SHARP_VIEWS_WINUI_NATIVE)
#define SKIA_SHARP_VIEWS_NATIVE_NAMESPACE winrt::SkiaSharp::Views::WinUI::Native
#else
#error A WinRT native platform namespace must be selected.
#endif
