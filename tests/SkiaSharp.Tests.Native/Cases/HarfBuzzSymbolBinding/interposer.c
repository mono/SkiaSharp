/*
 * Conflicting HarfBuzz exports loaded globally before the library under test.
 * Each function reports an unexpected internal call and exits with code 86,
 * detecting symbol preemption without relying on an incompatible library to crash.
 */
#include <stdio.h>
#include <stdlib.h>

static void preempted(const char *name)
{
    fprintf(stderr, "FAIL: bundled HarfBuzz called the global %s\n", name);
    exit(86);
}

void hb_ot_font_set_funcs(void *font)
{
    (void)font;
    preempted("hb_ot_font_set_funcs");
}

void hb_font_set_funcs(void *font, void *funcs, void *data, void (*destroy)(void *))
{
    (void)font;
    (void)funcs;
    (void)data;
    (void)destroy;
    preempted("hb_font_set_funcs");
}

/* Newer HarfBuzz initializes a font through this function instead. */
int hb_font_set_funcs_using(void *font, const char *name)
{
    (void)font;
    (void)name;
    preempted("hb_font_set_funcs_using");
    return 0;
}
