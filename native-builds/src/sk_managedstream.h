/*
 * Copyright 2015 Xamarin Inc.
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

// EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL EXPERIMENTAL
// DO NOT USE -- FOR INTERNAL TESTING ONLY

#ifndef sk_managedstream_DEFINED
#define sk_managedstream_DEFINED

#include "sk_xamarin.h"

#include "sk_types.h"

SK_C_PLUS_PLUS_BEGIN_GUARD


typedef struct sk_stream_managedstream_t sk_stream_managedstream_t;


typedef size_t                     (*sk_managedstream_read_delegate)         (sk_stream_managedstream_t* cmanagedStream, void* buffer, size_t size);
typedef bool                       (*sk_managedstream_isAtEnd_delegate)      (const sk_stream_managedstream_t* cmanagedStream);
typedef bool                       (*sk_managedstream_rewind_delegate)       (sk_stream_managedstream_t* cmanagedStream);
typedef size_t                     (*sk_managedstream_getPosition_delegate)  (const sk_stream_managedstream_t* cmanagedStream);
typedef bool                       (*sk_managedstream_seek_delegate)         (sk_stream_managedstream_t* cmanagedStream, size_t position);
typedef bool                       (*sk_managedstream_move_delegate)         (sk_stream_managedstream_t* cmanagedStream, long offset);
typedef size_t                     (*sk_managedstream_getLength_delegate)    (const sk_stream_managedstream_t* cmanagedStream);
typedef sk_stream_managedstream_t* (*sk_managedstream_createNew_delegate)    (const sk_stream_managedstream_t* cmanagedStream);
typedef void                       (*sk_managedstream_destroy_delegate)      (size_t cmanagedStream);


// c API
SK_API sk_stream_managedstream_t* sk_managedstream_new ();

SK_API void sk_managedstream_set_delegates (const sk_managedstream_read_delegate pRead,
                                            const sk_managedstream_isAtEnd_delegate pIsAtEnd,
                                            const sk_managedstream_rewind_delegate pRewind,
                                            const sk_managedstream_getPosition_delegate pGetPosition,
                                            const sk_managedstream_seek_delegate pSeek,
                                            const sk_managedstream_move_delegate pMove,
                                            const sk_managedstream_getLength_delegate pGetLength,
                                            const sk_managedstream_createNew_delegate pCreateNew,
                                            const sk_managedstream_destroy_delegate pDestroy);


SK_C_PLUS_PLUS_END_GUARD

#endif
