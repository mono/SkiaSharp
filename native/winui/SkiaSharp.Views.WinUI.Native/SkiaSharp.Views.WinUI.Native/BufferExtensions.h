#pragma once

#include "BufferExtensions.g.h"

using namespace winrt::Windows::Storage::Streams;

namespace winrt::SkiaSharp::Views::WinUI::Native::implementation
{
    struct BufferExtensions
    {
        BufferExtensions() = default;

        static intptr_t GetByteBuffer(IBuffer const& buffer);
    };
}

namespace winrt::SkiaSharp::Views::WinUI::Native::factory_implementation
{
    struct BufferExtensions : BufferExtensionsT<BufferExtensions, implementation::BufferExtensions>
    {
    };
}
