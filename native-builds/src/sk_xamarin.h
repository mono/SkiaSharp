/*
 * Copyright 2015 Xamarin Inc.
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

// EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL
// DO NOT USE -- FOR INTERNAL TESTING ONLY


#if defined(SKIA_DLL)
#  if defined(WIN32)
#    if SKIA_IMPLEMENTATION
#      define SK_API __declspec(dllexport)
#    else
#      define SK_API __declspec(dllimport)
#    endif
#  else
#    define SK_API __attribute__((visibility("default")))
#  endif
#else
#  define SK_API
#endif

