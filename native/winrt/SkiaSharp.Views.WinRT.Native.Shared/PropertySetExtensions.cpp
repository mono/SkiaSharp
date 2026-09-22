#include "pch.h"
#include "PropertySetExtensions.h"
#if __has_include("PropertySetExtensions.g.cpp")
#include "PropertySetExtensions.g.cpp"
#endif

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Foundation::Collections;

namespace SKIA_SHARP_VIEWS_NATIVE_NAMESPACE::implementation
{
    void PropertySetExtensions::AddSingle(PropertySet const& propertySet, hstring const& key, float value)
    {
        propertySet.Insert(key, PropertyValue::CreateSingle(value));
    }

    void PropertySetExtensions::AddSize(PropertySet const& propertySet, hstring const& key, Size const& value)
    {
        propertySet.Insert(key, PropertyValue::CreateSize(value));
    }
}
