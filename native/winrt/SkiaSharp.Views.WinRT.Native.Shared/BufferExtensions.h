#pragma once

#include "BufferExtensions.g.h"
#include "NativeNamespace.h"

using namespace winrt::Windows::Storage::Streams;

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::implementation
{
    struct BufferExtensions
    {
        BufferExtensions() = default;

        static intptr_t GetByteBuffer(IBuffer const& buffer);
    };
}

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::factory_implementation
{
    struct BufferExtensions : BufferExtensionsT<BufferExtensions, implementation::BufferExtensions>
    {
    };
}
