#!/usr/bin/env python3
"""Regression test for #3038: keep bundled HarfBuzz calls inside their library.

Run on a Linux host matching the native library's architecture:
    python3 scripts/infra/native/linux/test-harfbuzz-symbol-binding.py \
        output/native/linux/x64/libHarfBuzzSharp.so

Requires a C compiler, but no system HarfBuzz, font files, or .NET runtime.
Each probe runs in a fresh process so an earlier load cannot mask preemption.
"""

import argparse
import os
from pathlib import Path
import shlex
import subprocess
import tempfile


SOURCE = r"""
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

// Newer HarfBuzz versions initialize a font through this function instead.
int hb_font_set_funcs_using(void *font, const char *name)
{
    (void)font;
    (void)name;
    fputs("FAIL: bundled HarfBuzz called the global hb_font_set_funcs_using\n", stderr);
    exit(86);
}
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
"""


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("library", type=lambda path: Path(path).resolve(strict=True))
    parser.add_argument("--cc", default=os.environ.get("CC", "cc"))
    args = parser.parse_args()
    compiler = shlex.split(args.cc)
    probe_env = os.environ.copy()
    probe_env.pop("LD_PRELOAD", None)

    with tempfile.TemporaryDirectory(prefix="harfbuzz-symbol-binding-") as temp:
        directory = Path(temp)
        source = directory / "probe.c"
        source.write_text(SOURCE)
        probe = directory / "probe"
        interposer = directory / "interposer.so"
        subprocess.run(
            compiler + ["-Wall", "-Wextra", str(source), "-ldl", "-o", str(probe)],
            check=True,
        )
        subprocess.run(
            compiler + ["-Wall", "-Wextra", "-shared", "-fPIC", "-DINTERPOSER", str(source), "-o", str(interposer)],
            check=True,
        )

        print("Control: bundled library alone", flush=True)
        subprocess.run([str(probe), str(args.library)], env=probe_env, check=True)
        print("Regression: conflicting function loaded globally first", flush=True)
        subprocess.run([str(probe), str(args.library), str(interposer)], env=probe_env, check=True)


if __name__ == "__main__":
    main()
