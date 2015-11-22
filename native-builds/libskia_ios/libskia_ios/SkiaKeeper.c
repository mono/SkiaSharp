//
//  SkiaKeeper.c
//  libskia_ios
//
//  Created by Bill Holmes on 11/21/15.
//  Copyright © 2015 Xamarin. All rights reserved.
//

#include "SkiaKeeper.h"

#include "sk_surface.h"
#include "sk_paint.h"
#include "sk_typeface.h"



void** KeepSkiaCSymbols ()
{
    static void* ret[3] = {sk_surface_unref, sk_paint_new, sk_typeface_unref};
    
    return ret;
}
