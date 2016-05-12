/*
 * Copyright 2016 Nikita Tsukanov <keks9n@gmail.com>
 *
 * Initial implementation imported from Perspex https://github.com/Perspex/libperspesk
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#ifndef SkRenderTarget_DEFINED
#define SkRenderTarget_DEFINED
#include "SkTypes.h"
#include "SkCanvas.h"

class SkRenderContext
{
public:
    SkCanvas* Canvas;
    virtual ~SkRenderContext () {}
};

class SkRenderTarget
{
public:
    virtual ~SkRenderTarget ()
    {
    }

    virtual SkRenderContext* CreateRenderContext () = 0;
};


#endif