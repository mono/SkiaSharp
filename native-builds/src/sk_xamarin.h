/*
 * Copyright 2015 Xamarin Inc.
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

// EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL
// DO NOT USE -- FOR INTERNAL TESTING ONLY


#if defined(_WIN32)
#  define SK_X_API __declspec(dllexport)
#else
#  define SK_X_API __attribute__((visibility("default")))
#endif
