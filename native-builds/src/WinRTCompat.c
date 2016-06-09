//
//  WinRTCompat.c
//
//  Created by Matthew Leibowitz on 06/09/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#if defined(SK_BUILD_FOR_WINRT)

#include <windows.h>

void ExitProcess(UINT code)
{
    // we can't kill in WinRT
}

#if defined(_M_ARM)

// This should have been not used, but as the code is designed for x86
// and there is a RUNTIME check for simd, this has to exist. As the
// runtime check will fail, and revert to a C implementation, this is 
// not a problem to have a stub.
unsigned int _mm_crc32_u32(unsigned int crc, unsigned int v)
{
    return 0;
}

#endif

#endif
