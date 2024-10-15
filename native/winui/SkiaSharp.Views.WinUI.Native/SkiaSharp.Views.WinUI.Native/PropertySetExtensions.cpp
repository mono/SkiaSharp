#include "pch.h"
#include "PropertySetExtensions.h"
#include "PropertySetExtensions.g.cpp"

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Foundation::Collections;

namespace winrt::SkiaSharp::Views::WinUI::Native::implementation
{
    void PropertySetExtensions::AddSingle(PropertySet const& propertySet, hstring const& key, float value)
    {
        propertySet.Insert(key, PropertyValue::CreateSingle(value));
    }

    void PropertySetExtensions::AddSize(PropertySet const& propertySet, hstring const& key, Size const& height)
    {
        propertySet.Insert(key, PropertyValue::CreateSize(height));
    }
}
