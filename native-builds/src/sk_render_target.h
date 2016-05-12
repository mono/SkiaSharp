#ifndef sk_render_target_DEFINED
#define sk_render_Target_DEFINED

#include "sk_xamarin.h"
#include "sk_types.h"
SK_C_PLUS_PLUS_BEGIN_GUARD

typedef struct sk_render_target_t sk_render_target_t;
typedef struct sk_render_context_t sk_render_context_t;

SK_X_API sk_render_target_t* sk_os_window_render_target_new (void*, bool);
SK_X_API void sk_render_target_delete (sk_render_target_t*);
SK_X_API sk_render_context_t* sk_render_target_create_context (sk_render_target_t*);
SK_X_API sk_canvas_t* sk_render_context_get_canvas (sk_render_context_t*);
SK_X_API void sk_render_context_delete (sk_render_context_t*);

SK_C_PLUS_PLUS_END_GUARD
#endif