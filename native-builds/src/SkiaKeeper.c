//
//  SkiaKeeper.c
//
//  Created by Bill Holmes on 11/23/15.
//  Copyright © 2015 Xamarin. All rights reserved.
//

#include "sk_xamarin.h"

#include "sk_surface.h"
#include "sk_paint.h"
#include "sk_typeface.h"
#include "sk_stream.h"
#include "sk_managedstream.h"

SK_API void** KeepSkiaCSymbols ();

void** KeepSkiaCSymbols ()
{
    static void* ret[] = {
        (void*)sk_surface_unref,
        (void*)sk_paint_new,
        (void*)sk_typeface_unref,
        (void*)sk_filestream_new,
        (void*)sk_managedstream_new,
    };
    return ret;
}
