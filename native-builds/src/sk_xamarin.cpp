
#include "sk_xamarin.h"

#include "../images/SkForceLinking.h"

#ifdef NEED_INIT_NEON

namespace SkOpts {
    void Init_neon() {}
}

#endif

void sk_force_linking () {
    SkForceLinking(false);
}
