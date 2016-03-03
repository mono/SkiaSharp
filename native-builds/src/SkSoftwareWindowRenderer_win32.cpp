/*
 * Copyright 2016 Nikita Tsukanov <keks9n@gmail.com>
 *
 * Initial implementation imported from Perspex https://github.com/Perspex/libperspesk
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#include "SkOsWindowRenderTarget.h"
#ifdef SK_BUILD_FOR_WIN32
SkSoftwareWindowRenderer::SkSoftwareWindowRenderer (void* hWnd)
{
    _hWnd = hWnd;
}

void SkSoftwareWindowRenderer::Present ()
{
    BITMAPINFO bmi;
    memset(&bmi, 0, sizeof(bmi));
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth = _bitmap.width();
    bmi.bmiHeader.biHeight = -_bitmap.height(); // top-down image
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biBitCount = 32;
    bmi.bmiHeader.biCompression = BI_RGB;
    bmi.bmiHeader.biSizeImage = 0;



    HDC hdc = GetDC((HWND)_hWnd);
    _bitmap.lockPixels();
    int ret = SetDIBitsToDevice(hdc,
        0, 0,
        _bitmap.width(), _bitmap.height(),
        0, 0,
        0, _bitmap.height(),
        _bitmap.getPixels(),
        &bmi,
        DIB_RGB_COLORS);
    _bitmap.unlockPixels();

    ReleaseDC((HWND)_hWnd, hdc);
}

void SkSoftwareWindowRenderer::Prepare ()
{
    //Do nothing
}

void SkSoftwareWindowRenderer::GetDimensions (int& width, int& height) const
{
    RECT rc;
    GetClientRect((HWND)_hWnd, &rc);
    width = rc.right - rc.left;
    height = rc.bottom - rc.top;
}

void SkSoftwareWindowRenderer::ReziseSurface (int width, int height)
{
    _surface.reset(nullptr);
    _bitmap.allocPixels(SkImageInfo::MakeN32(width, height, kPremul_SkAlphaType));
    _surface.reset(SkSurface::NewRasterDirect(_bitmap.info(), _bitmap.getPixels(), _bitmap.rowBytes()));
}
#endif