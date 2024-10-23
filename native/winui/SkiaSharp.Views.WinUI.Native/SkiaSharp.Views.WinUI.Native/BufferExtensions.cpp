#include "pch.h"
#include "BufferExtensions.h"
#include "BufferExtensions.g.cpp"

using namespace winrt::Windows::Storage::Streams;

namespace winrt::SkiaSharp::Views::WinUI::Native::implementation
{
    intptr_t BufferExtensions::GetByteBuffer(IBuffer const& buffer)
    {
        byte* current_data = nullptr;
        auto bufferByteAccess = buffer.as<winrt::impl::IBufferByteAccess>();
        bufferByteAccess->Buffer(&current_data);
        return (intptr_t)current_data;
    }
}
