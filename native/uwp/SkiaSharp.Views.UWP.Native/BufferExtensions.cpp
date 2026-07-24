#include "pch.h"
#include "BufferExtensions.h"
#if __has_include("BufferExtensions.g.cpp")
#include "BufferExtensions.g.cpp"
#endif

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using namespace winrt::Windows::Storage::Streams;
namespace winrt::SkiaSharp::Views::UWP::Native::implementation
{
    intptr_t BufferExtensions::GetByteBuffer(IBuffer const& buffer)
    {
        byte* current_data = nullptr;
        auto bufferByteAccess = buffer.as<winrt::impl::IBufferByteAccess>();
        bufferByteAccess->Buffer(&current_data);
        return (intptr_t)current_data;
    }
}
