#include "pch.h"
#include "BufferExtensions.h"
#if __has_include("BufferExtensions.g.cpp")
#include "BufferExtensions.g.cpp"
#endif

using namespace winrt::Windows::Storage::Streams;

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::implementation
{
    intptr_t BufferExtensions::GetByteBuffer(IBuffer const& buffer)
    {
        byte* currentData = nullptr;
        auto bufferByteAccess = buffer.as<winrt::impl::IBufferByteAccess>();
        bufferByteAccess->Buffer(&currentData);
        return reinterpret_cast<intptr_t>(currentData);
    }
}
