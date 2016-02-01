/*
 * Copyright 2016 Xamarin Inc.
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#include "SkManagedStream.h"

#include "sk_managedstream.h"
#include "sk_types_priv.h"


static sk_managedstream_read_delegate         gRead;
static sk_managedstream_isAtEnd_delegate      gIsAtEnd;
static sk_managedstream_rewind_delegate       gRewind;
static sk_managedstream_getPosition_delegate  gGetPosition;
static sk_managedstream_seek_delegate         gSeek;
static sk_managedstream_move_delegate         gMove;
static sk_managedstream_getLength_delegate    gGetLength;
static sk_managedstream_createNew_delegate    gCreateNew;
static sk_managedstream_destroy_delegate      gDestroy;


static inline SkManagedStream* AsManagedStream(sk_stream_managedstream_t* cstream) {
    return reinterpret_cast<SkManagedStream*>(cstream);
}


size_t dRead(SkManagedStream* managedStream, void* buffer, size_t size)
{
    return gRead((sk_stream_managedstream_t*)managedStream, buffer, size);
}
bool dIsAtEnd(const SkManagedStream* managedStream)
{
    return gIsAtEnd((sk_stream_managedstream_t*)managedStream);
}
bool dRewind(SkManagedStream* managedStream)
{
    return gRewind((sk_stream_managedstream_t*)managedStream);
}
size_t dGetPosition(const SkManagedStream* managedStream)
{
    return gGetPosition((sk_stream_managedstream_t*)managedStream);
}
bool dSeek(SkManagedStream* managedStream, size_t position)
{
    return gSeek((sk_stream_managedstream_t*)managedStream, position);
}
bool dMove(SkManagedStream* managedStream, long offset)
{
    return gMove((sk_stream_managedstream_t*)managedStream, offset);
}
size_t dGetLength(const SkManagedStream* managedStream)
{
    return gGetLength((sk_stream_managedstream_t*)managedStream);
}
SkManagedStream* dCreateNew(const SkManagedStream* managedStream)
{
    return AsManagedStream(gCreateNew((sk_stream_managedstream_t*)managedStream));
}
void dDestroy(size_t managedStream)
{
    gDestroy(managedStream);
}


sk_stream_managedstream_t* sk_managedstream_new ()
{
    return (sk_stream_managedstream_t*)new SkManagedStream();
}

void sk_managedstream_set_delegates (const sk_managedstream_read_delegate pRead,
                                     const sk_managedstream_isAtEnd_delegate pIsAtEnd,
                                     const sk_managedstream_rewind_delegate pRewind,
                                     const sk_managedstream_getPosition_delegate pGetPosition,
                                     const sk_managedstream_seek_delegate pSeek,
                                     const sk_managedstream_move_delegate pMove,
                                     const sk_managedstream_getLength_delegate pGetLength,
                                     const sk_managedstream_createNew_delegate pCreateNew,
                                     const sk_managedstream_destroy_delegate pDestroy)
{
    gRead = pRead;
    gIsAtEnd = pIsAtEnd;
    gRewind = pRewind;
    gGetPosition = pGetPosition;
    gSeek = pSeek;
    gMove = pMove;
    gGetLength = pGetLength;
    gCreateNew = pCreateNew;
    gDestroy = pDestroy;

    SkManagedStream::setDelegates(dRead, dIsAtEnd, dRewind, dGetPosition, dSeek, dMove, dGetLength, dCreateNew, dDestroy);
}

