/*
 * Copyright 2016 Nikita Tsukanov <keks9n@gmail.com>
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */


#include "SkRenderTarget.h"
#include "sk_render_target.h"

static inline SkRenderTarget* AsRenderTarget(sk_render_target_t* ctarget) {
    return reinterpret_cast<SkRenderTarget*>(ctarget);
}

static inline SkRenderContext* AsRenderContext(sk_render_context_t* ccontext) {
    return reinterpret_cast<SkRenderContext*>(ccontext);
}

void sk_render_target_delete (sk_render_target_t*ctarget)
{
    delete AsRenderTarget (ctarget);
}

sk_render_context_t* sk_render_target_create_context(sk_render_target_t*ctarget)
{
    return reinterpret_cast<sk_render_context_t*>(AsRenderTarget (ctarget)->CreateRenderContext ());
}

sk_canvas_t* sk_render_context_get_canvas(sk_render_context_t*ctx)
{
    return reinterpret_cast<sk_canvas_t*>(AsRenderContext (ctx)->Canvas);
}

void sk_render_context_delete(sk_render_context_t*ctx)
{
    delete AsRenderContext (ctx);
}