#pragma once

#include "PropertySetExtensions.g.h"
#include "NativeNamespace.h"

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Foundation::Collections;

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::implementation
{
    struct PropertySetExtensions
    {
        PropertySetExtensions() = default;

        static void AddSingle(PropertySet const& propertySet, hstring const& key, float value);
        static void AddSize(PropertySet const& propertySet, hstring const& key, Size const& value);
    };
}

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::factory_implementation
{
    struct PropertySetExtensions : PropertySetExtensionsT<PropertySetExtensions, implementation::PropertySetExtensions>
    {
    };
}
