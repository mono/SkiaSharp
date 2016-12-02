//
//  SkManagedStream.cpp
//
//  Created by Matthew on 2016/01/08.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "SkManagedStream.h"


static read_delegate fRead = nullptr;
static peek_delegate fPeek = nullptr;
static isAtEnd_delegate fIsAtEnd = nullptr;
static rewind_delegate fRewind = nullptr;
static getPosition_delegate fGetPosition = nullptr;
static seek_delegate fSeek = nullptr;
static move_delegate fMove = nullptr;
static getLength_delegate fGetLength = nullptr;
static createNew_delegate fCreateNew = nullptr;
static destroy_delegate fDestroy = nullptr;


SkManagedStream::SkManagedStream() {
    this->address = (size_t)this;
}

SkManagedStream::~SkManagedStream() {
    fDestroy(address);
}

void SkManagedStream::setDelegates(const read_delegate pRead,
                                   const peek_delegate pPeek,
                                   const isAtEnd_delegate pIsAtEnd,
                                   const rewind_delegate pRewind,
                                   const getPosition_delegate pGetPosition,
                                   const seek_delegate pSeek,
                                   const move_delegate pMove,
                                   const getLength_delegate pGetLength,
                                   const createNew_delegate pCreateNew,
                                   const destroy_delegate pDestroy)
{
    fRead = (pRead);
    fPeek = (pPeek);
    fIsAtEnd = (pIsAtEnd);
    fRewind = (pRewind);
    fGetPosition = (pGetPosition);
    fSeek = (pSeek);
    fMove = (pMove);
    fGetLength = (pGetLength);
    fCreateNew = (pCreateNew);
    fDestroy = (pDestroy);
}


size_t SkManagedStream::read(void* buffer, size_t size) {
    return fRead(this, buffer, size);
}

size_t SkManagedStream::peek(void *buffer, size_t size) const {
    SkManagedStream* nonConstThis = const_cast<SkManagedStream*>(this);
    return fPeek(nonConstThis, buffer, size);
}

bool SkManagedStream::isAtEnd() const {
    return fIsAtEnd(this);
}

bool SkManagedStream::rewind() {
    return fRewind(this);
}

size_t SkManagedStream::getPosition() const {
    return fGetPosition(this);
}

bool SkManagedStream::seek(size_t position) {
    return fSeek(this, position);
}

bool SkManagedStream::move(long offset) {
    return fMove(this, offset);
}

size_t SkManagedStream::getLength() const {
    return fGetLength(this);
}

SkManagedStream* SkManagedStream::duplicate() const {
    return fCreateNew(this);
}

SkManagedStream* SkManagedStream::fork() const {
    std::unique_ptr<SkManagedStream> that(fCreateNew(this));
    that->seek(getPosition());
    return that.release();
}


