#ifndef ENTRYPOINTS_H
#define ENTRYPOINTS_H

#include <stdint.h>
#include <stddef.h>

#if defined(_WIN32)
#   if defined(INTEROP_LIBRARY)
#       define API_EXPORT __declspec(dllexport)
#   else
#       define API_EXPORT __declspec(dllimport)
#   endif
#elif defined(__GNUC__)
#   if defined(INTEROP_LIBRARY)
#       define API_EXPORT __attribute__((visibility ("default")))
#   else
#       define API_EXPORT
#   endif
#else
#   define API_EXPORT
#endif

#ifdef __cplusplus
#   define SK_C_PLUS_PLUS_BEGIN_GUARD    extern "C" {
#   define SK_C_PLUS_PLUS_END_GUARD      }
#else
#   include <stdbool.h>
#   define SK_C_PLUS_PLUS_BEGIN_GUARD
#   define SK_C_PLUS_PLUS_END_GUARD
#endif

#include <windows.foundation.h>

SK_C_PLUS_PLUS_BEGIN_GUARD

API_EXPORT void PropertySetInterop_AddSingle(ABI::Windows::Foundation::Collections::IPropertySet* propertySet, HSTRING key, FLOAT value);
API_EXPORT void PropertySetInterop_AddSize(ABI::Windows::Foundation::Collections::IPropertySet* propertySet, HSTRING key, FLOAT width, FLOAT height);

SK_C_PLUS_PLUS_END_GUARD

#endif // ENTRYPOINTS_H
