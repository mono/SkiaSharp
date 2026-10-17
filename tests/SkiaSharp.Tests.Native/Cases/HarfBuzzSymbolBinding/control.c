/*
 * Minimal fake HarfBuzz library with an intentionally preemptible internal call.
 * It passes alone but must fail with the interposer, proving the regression probe
 * detects incorrect binding rather than passing because the conflict is inactive.
 */
#include <stddef.h>

static int face;

void hb_font_set_funcs(void *font, void *funcs, void *data, void (*destroy)(void *))
{
    (void)font;
    (void)funcs;
    (void)data;
    (void)destroy;
}

void *hb_face_get_empty(void)
{
    return &face;
}

void *hb_font_create(void *value)
{
    /* This call must remain preemptible so the negative control proves the probe works. */
    hb_font_set_funcs(value, NULL, NULL, NULL);
    return value;
}

void hb_font_destroy(void *font)
{
    (void)font;
}

void hb_ot_font_set_funcs(void *font)
{
    (void)font;
}
