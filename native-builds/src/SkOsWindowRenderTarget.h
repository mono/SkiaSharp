/*
 * Copyright 2016 Nikita Tsukanov <keks9n@gmail.com>
 *
 * Initial implementation imported from Perspex https://github.com/Perspex/libperspesk
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#ifndef SkOsWindowRenderTarget_DEFINED
#define SkOsWindowRenderTarget_DEFINED
#include "SkTypes.h"
#include "SkCanvas.h"
#include "SkSurface.h"
#include "SkRenderTarget.h"

class SkSoftwareWindowRenderer
{
    SkAutoTUnref<SkSurface> _surface;
    SkBitmap _bitmap;
    void* _hWnd;
public:
    SkSoftwareWindowRenderer(void* hWnd);
    //Presents window contents on screen
    void Present ();
    //Needed for platforms like android where you need to call ANativeWindow_lock before rendering
    void Prepare ();
    SkSurface* getSurface () const
    {
        return _surface.get ();
    }
    //Obtain actual window dimensions
    void GetDimensions (int& width, int& height) const;

    //Resize underlying surface
    void ReziseSurface (int width, int height);
};

class GlWindowRenderer
{
public:
    virtual ~GlWindowRenderer ()
    {
    }

    virtual SkSurface* getSurface () = 0;
    virtual void Detach () = 0;
    virtual bool Attach (int width, int height, int i) = 0;
    virtual void Present () = 0;
    virtual void MakeCurrent () = 0;
};

class SkOsWindowRenderTarget : public SkRenderTarget
{
    bool _softwareOnly;
    GlWindowRenderer* _gl;
    void* _hWnd;
    SkSoftwareWindowRenderer _sw;
    bool _isGpu;
    int _width, _height;
protected:
    void FixSize();
public:
    SkSurface* getSurface();
    SkOsWindowRenderTarget(void* hWnd, bool softwareOnly);
    void Present();
    ~SkOsWindowRenderTarget();
    virtual SkRenderContext* CreateRenderContext() override;
};

#endif
