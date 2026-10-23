/*
 * Standalone loader for the HarfBuzz symbol-binding regression. It creates a font
 * with or without a globally loaded interposer, keeping each load order isolated
 * in a fresh process so native failures cannot terminate the xUnit host.
 */
#define _GNU_SOURCE
#include <dlfcn.h>
#include <stdio.h>
#include <stdlib.h>

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
    if (argc != 2 && argc != 3) {
        fputs("Usage: probe library [interposer]\n", stderr);
        return 2;
    }

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
