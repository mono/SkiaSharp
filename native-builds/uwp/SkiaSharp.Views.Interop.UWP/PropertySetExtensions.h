#pragma once

#include <stdint.h>
#include <stddef.h>

#if defined(_WIN32)
#    define API_EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
#    define API_EXPORT __attribute__((visibility ("default")))
#else
#    define API_EXPORT
#endif

#ifdef __cplusplus
#    define C_PLUS_PLUS_BEGIN_GUARD    extern "C" {
#    define C_PLUS_PLUS_END_GUARD      }
#else
#    define C_PLUS_PLUS_BEGIN_GUARD
#    define C_PLUS_PLUS_END_GUARD
#endif

#include <windows.foundation.h>

using IPropertySet = ABI::Windows::Foundation::Collections::IPropertySet;

C_PLUS_PLUS_BEGIN_GUARD

API_EXPORT void PropertySet_AddSingle(IPropertySet* propertySet, HSTRING key, FLOAT value);
API_EXPORT void PropertySet_AddSize(IPropertySet* propertySet, HSTRING key, FLOAT width, FLOAT height);

C_PLUS_PLUS_END_GUARD
