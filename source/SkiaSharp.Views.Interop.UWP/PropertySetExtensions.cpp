#include "pch.h"
#include "PropertySetExtensions.h"

#include <windows.foundation.h>
#include <windows.ui.notifications.h>
#include <wrl.h>

using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace ABI::Windows::Foundation;
using namespace ABI::Windows::Foundation::Collections;

using IPropertySet = ABI::Windows::Foundation::Collections::IPropertySet;

void PropertySet_AddSingle(IPropertySet* propertySet, HSTRING key, FLOAT value)
{
    ComPtr<IPropertySet> propSet = propertySet;
    ComPtr<IMap<HSTRING, IInspectable*>> map;
    propSet.As(&map);

    ComPtr<IPropertyValueStatics> propValueFactory;
    GetActivationFactory(HStringReference(RuntimeClass_Windows_Foundation_PropertyValue).Get(), &propValueFactory);

    ComPtr<IInspectable> valueInspectable;
    propValueFactory->CreateSingle(value, &valueInspectable);

    boolean replaced;
    map->Insert(key, valueInspectable.Get(), &replaced);
}

void PropertySet_AddSize(IPropertySet* propertySet, HSTRING key, FLOAT width, FLOAT height)
{
    ComPtr<IPropertySet> propSet = propertySet;
    ComPtr<IMap<HSTRING, IInspectable*>> map;
    propSet.As(&map);

    ComPtr<IPropertyValueStatics> propValueFactory;
    GetActivationFactory(HStringReference(RuntimeClass_Windows_Foundation_PropertyValue).Get(), &propValueFactory);

    ComPtr<IInspectable> valueInspectable;
    Size size = { width, height };
    propValueFactory->CreateSize(size, &valueInspectable);

    boolean replaced;
    map->Insert(key, valueInspectable.Get(), &replaced);
}
