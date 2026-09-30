#pragma once

#include "BufferExtensions.g.h"
using namespace winrt::Windows::Storage::Streams;
namespace winrt::SkiaSharp::Views::UWP::Native::implementation
{
    struct BufferExtensions
    {
        BufferExtensions() = default;

        static intptr_t GetByteBuffer(IBuffer const& buffer);
    };
}
// winrt::SkiaSharp_Views_UWP_Native::
namespace winrt::SkiaSharp::Views::UWP::Native::factory_implementation
{
    struct BufferExtensions : BufferExtensionsT<BufferExtensions, implementation::BufferExtensions>
    {
    };
}
