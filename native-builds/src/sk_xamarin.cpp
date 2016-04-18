/*
 * Copyright 2016 Xamarin Inc.
 *
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

#include "sk_xamarin.h"

#include "sk_types_priv.h"

#include "../images/SkForceLinking.h"

#ifdef NEED_INIT_NEON

namespace SkOpts {
    void Init_neon() {}
}

#endif

void sk_force_linking () {
    SkForceLinking(false);
}
