#pragma once

#include "PropertySetExtensions.g.h"

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Foundation::Collections;

namespace winrt::SkiaSharp::Views::WinUI::Native::implementation
{
    struct PropertySetExtensions
    {
        PropertySetExtensions() = default;

        static void AddSingle(PropertySet const& propertySet, hstring const& key, float value);
        static void AddSize(PropertySet const& propertySet, hstring const& key, Size const& value);
    };
}

namespace winrt::SkiaSharp::Views::WinUI::Native::factory_implementation
{
    struct PropertySetExtensions : PropertySetExtensionsT<PropertySetExtensions, implementation::PropertySetExtensions>
    {
    };
}
