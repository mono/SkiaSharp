#define _GNU_SOURCE
#include <dlfcn.h>
#include <stdio.h>
#include <stdlib.h>

#ifdef INTERPOSER
void hb_ot_font_set_funcs(void *font)
{
    (void)font;
    fputs("FAIL: bundled HarfBuzz called the global hb_ot_font_set_funcs\n", stderr);
    exit(86);
}

void hb_font_set_funcs(void *font, void *funcs, void *data, void (*destroy)(void *))
{
    (void)font;
    (void)funcs;
    (void)data;
    (void)destroy;
    fputs("FAIL: bundled HarfBuzz called the global hb_font_set_funcs\n", stderr);
    exit(86);
}

/* Newer HarfBuzz initializes a font through this function instead. */
int hb_font_set_funcs_using(void *font, const char *name)
{
    (void)font;
    (void)name;
    fputs("FAIL: bundled HarfBuzz called the global hb_font_set_funcs_using\n", stderr);
    exit(86);
}
#elif defined(NEGATIVE_CONTROL)
static int fake_face;

void hb_font_set_funcs(void *font, void *funcs, void *data, void (*destroy)(void *))
{
    (void)font;
    (void)funcs;
    (void)data;
    (void)destroy;
}

void *hb_face_get_empty(void) { return &fake_face; }

void *hb_font_create(void *face)
{
    /* Keep this call preemptible to verify that the interposer is active. */
    hb_font_set_funcs(face, NULL, NULL, NULL);
    return face;
}

void hb_font_destroy(void *font) { (void)font; }
void hb_ot_font_set_funcs(void *font) { (void)font; }
#else
static void *load(const char *path, int flags)
{
    void *handle = dlopen(path, flags);
    if (!handle) {
        fprintf(stderr, "dlopen(%s): %s\n", path, dlerror());
        exit(2);
    }
    return handle;
}

static void *symbol(void *handle, const char *name)
{
    dlerror();
    void *result = dlsym(handle, name);
    const char *error = dlerror();
    if (error) {
        fprintf(stderr, "dlsym(%s): %s\n", name, error);
        exit(2);
    }
    return result;
}

int main(int argc, char **argv)
{
    if (argc != 2 && argc != 3)
        return 2;
    if (argc == 3)
        load(argv[2], RTLD_NOW | RTLD_GLOBAL);
    void *library = load(argv[1], RTLD_NOW | RTLD_LOCAL);
    void *(*face_empty)(void) = symbol(library, "hb_face_get_empty");
    void *(*font_create)(void *) = symbol(library, "hb_font_create");
    void (*font_destroy)(void *) = symbol(library, "hb_font_destroy");
    void (*set_ot_funcs)(void *) = symbol(library, "hb_ot_font_set_funcs");
    void *font = font_create(face_empty());
    if (!font) {
        fputs("FAIL: hb_font_create returned null\n", stderr);
        return 3;
    }
    set_ot_funcs(font);
    font_destroy(font);
    puts("PASS: bundled font creation and OpenType functions");
    return 0;
}
#endif
