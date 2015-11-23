

#include "sk_surface.h"
#include "sk_paint.h"
#include "sk_typeface.h"

__attribute__((visibility("default"))) void** KeepSkiaCSymbols ();


void** KeepSkiaCSymbols ()
{
    static void* ret[3] = {(void*)sk_surface_unref, (void*)sk_paint_new, (void*)sk_typeface_unref};

    return ret;
}

#ifdef NEED_INIT_NEON

namespace SkOpts {
    void Init_neon() {}
}

#endif