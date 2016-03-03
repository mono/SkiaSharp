/*
 * Copyright 2016 Nikita Tsukanov <keks9n@gmail.com>
 *
 * Initial implementation imported from Perspex https://github.com/Perspex/libperspesk
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#include "SkOsWindowRenderTarget.h"
#include "sk_render_target.h"

void SkOsWindowRenderTarget::FixSize ()
{
    int width, height;
    _sw.GetDimensions (width, height);
    if (_width == width && _height == height)
        return;
    _width = width;
    _height = height;

    //Free resources
    if (_isGpu)
    {
        _gl->Detach();
        _isGpu = false;
    }

    //Initialize
    if (!_softwareOnly)
    {
        _isGpu = _gl->Attach(width, height, 0);
    }
    if (!_isGpu)
    {
        _sw.ReziseSurface(width, height);
    }
}

SkSurface* SkOsWindowRenderTarget::getSurface ()
{
    if (_isGpu)
        return _gl->getSurface();
    return _sw.getSurface ();
};

class SkNullGlWindowRenderer : public GlWindowRenderer
{
public:
    SkSurface* getSurface () override
    {
        return nullptr;
    }
    void Detach () override
    {
        //
    }
    bool Attach (int width, int height, int i) override
    {
        return false;
    }
    void Present () override
    {
        //
    }
    void MakeCurrent () override
    {
        //
    }
};


SkOsWindowRenderTarget::SkOsWindowRenderTarget (void* hWnd, bool softwareOnly) : _sw(hWnd)
{
    _softwareOnly = softwareOnly;
    //TODO: Create an actual renderer instance once it's ported from Perspex
    _gl = new SkNullGlWindowRenderer();
    this->_hWnd = hWnd;
    _width = 0, _height = 0;
    _isGpu = false;
    FixSize();
}

void SkOsWindowRenderTarget::Present ()
{
    if (_isGpu)
        _gl->Present();
    else
        _sw.Present();
}

class WinContext : public SkRenderContext
{
    SkOsWindowRenderTarget*Target;
public:
    WinContext(SkOsWindowRenderTarget* target)
    {
        Target = target;
        Canvas = target->getSurface()->getCanvas();
    }

    ~WinContext()
    {
        Target->getSurface()->getCanvas()->flush();
        Target->Present();
    }
};

SkOsWindowRenderTarget::~SkOsWindowRenderTarget ()
{
    if (_isGpu)
        _gl->Detach();
}

SkRenderContext* SkOsWindowRenderTarget::CreateRenderContext ()
{
    FixSize();
    if (_isGpu)
        _gl->MakeCurrent();
    else
        _sw.Prepare();
    SkSurface*s = getSurface();
    if (s == nullptr)
        SkDebugf("No surface to draw, crashing...\n");
    s->getCanvas()->restoreToCount(1);
    s->getCanvas()->save();
    s->getCanvas()->clear(SkColorSetARGB(0, 0, 0, 0));
    s->getCanvas()->resetMatrix();
    return new WinContext(this);
};


sk_render_target_t* sk_os_window_render_target_new (void*window, bool softwareOnly)
{
    return (sk_render_target_t*) new SkOsWindowRenderTarget(window, softwareOnly);
}