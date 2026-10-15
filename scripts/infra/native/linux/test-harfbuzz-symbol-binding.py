#!/usr/bin/env python3
"""Regression test for #3038: keep bundled HarfBuzz calls inside their library.

Run on a Linux host matching the native library's architecture:
    python3 scripts/infra/native/linux/test-harfbuzz-symbol-binding.py \
        output/native/linux/x64/libHarfBuzzSharp.so

Requires a C compiler, but no system HarfBuzz, font files, or .NET runtime.
Each probe runs in a fresh process so an earlier load cannot mask preemption.
An intentionally preemptible fake library verifies the interposer first.
Every compilation and probe has a 60-second timeout.
Set TMPDIR to select a writable, executable scratch directory if needed.
"""

import argparse
import os
from pathlib import Path
import shlex
import signal
import subprocess
import sys
import tempfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "tests"))
from native_process import COMMAND_TIMEOUT, run_process


TIMEOUT = COMMAND_TIMEOUT
INHERITED_PROCESS_GROUP = False
INTERPOSITION_MESSAGE = "FAIL: bundled HarfBuzz called the global hb_font_set_funcs"


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
#elif defined(NEGATIVE_CONTROL)
static int fake_face;

void hb_font_set_funcs(void *font, void *funcs, void *data, void (*destroy)(void *))
{
    (void)font;
    (void)funcs;
    (void)data;
    (void)destroy;
}

void *hb_face_get_empty(void)
{
    return &fake_face;
}

void *hb_font_create(void *face)
{
    // This call must remain preemptible to verify that the interposer is active.
    hb_font_set_funcs(face, NULL, NULL, NULL);
    return face;
}

void hb_font_destroy(void *font)
{
    (void)font;
}

void hb_ot_font_set_funcs(void *font)
{
    (void)font;
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


def run_command(command, env, expected=0):
    try:
        result = run_process(
            command, env=env, timeout=TIMEOUT,
            new_process_group=not INHERITED_PROCESS_GROUP,
        )
    except subprocess.TimeoutExpired as error:
        raise RuntimeError(
            f"Command timed out after {TIMEOUT}s: {command!r}\n"
            f"stdout: {error.stdout!r}\nstderr: {error.stderr!r}"
        ) from error
    except OSError as error:
        raise RuntimeError(f"Could not execute {command!r}: {error}") from error
    if result.returncode != expected:
        raise RuntimeError(
            f"Command returned {result.returncode}, expected {expected}: {command!r}\n"
            f"stdout:\n{result.stdout}\nstderr:\n{result.stderr}"
        )
    if result.stdout:
        print(result.stdout, end="", flush=True)
    if result.stderr:
        print(result.stderr, end="", file=sys.stderr, flush=True)
    return result


def verify_negative_control(probe, negative, interposer, env):
    print("Negative control: intentionally preemptible fake HarfBuzz alone", flush=True)
    run_command([str(probe), str(negative)], env)
    print("Negative control: interposer must terminate fake HarfBuzz", flush=True)
    result = run_command([str(probe), str(negative), str(interposer)], env, expected=86)
    if INTERPOSITION_MESSAGE not in result.stderr:
        raise RuntimeError(
            f"Negative control exited 86 without the interposer diagnostic:\n{result.stderr}"
        )
    print("PASS: negative control was interposed (exit 86)", flush=True)


def main():
    global INHERITED_PROCESS_GROUP
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("library", type=lambda path: Path(path).resolve(strict=True))
    parser.add_argument("--cc", default=os.environ.get("CC", "cc"))
    parser.add_argument("--inherited-process-group", action="store_true", help=argparse.SUPPRESS)
    args = parser.parse_args()
    if not args.library.is_file() or not args.library.stat().st_size:
        parser.error(f"Library is missing, empty, or not a file: {args.library}")
    compiler = shlex.split(args.cc)
    if not compiler:
        parser.error("--cc must name a C compiler")
    INHERITED_PROCESS_GROUP = args.inherited_process_group
    if INHERITED_PROCESS_GROUP and (
        os.name != "posix" or os.getpgrp() != os.getpid() or os.getsid(0) != os.getpid()
    ):
        parser.error("--inherited-process-group requires an owning process-group wrapper")
    probe_env = os.environ.copy()
    probe_env.pop("LD_PRELOAD", None)
    probe_env.pop("LD_AUDIT", None)
    probe_env["LC_ALL"] = "C"

    previous_handler = None
    if INHERITED_PROCESS_GROUP:
        def terminated(signum, frame):
            raise RuntimeError("Runtime harness terminated; cleaning up probe artifacts")
        previous_handler = signal.signal(signal.SIGTERM, terminated)
    try:
        run_probes(args.library, compiler, probe_env)
    finally:
        if previous_handler is not None:
            signal.signal(signal.SIGTERM, previous_handler)


def run_probes(library, compiler, probe_env):
    with tempfile.TemporaryDirectory(prefix=".harfbuzz-symbol-binding-") as temp:
        directory = Path(temp)
        source = directory / "probe.c"
        source.write_text(SOURCE)
        probe = directory / "probe"
        interposer = directory / "interposer.so"
        negative = directory / "negative-control.so"
        run_command(
            compiler + ["-Wall", "-Wextra", str(source), "-ldl", "-o", str(probe)],
            probe_env,
        )
        run_command(
            compiler + ["-Wall", "-Wextra", "-shared", "-fPIC", "-DINTERPOSER", str(source), "-o", str(interposer)],
            probe_env,
        )
        run_command(
            compiler + ["-Wall", "-Wextra", "-shared", "-fPIC", "-DNEGATIVE_CONTROL", str(source), "-o", str(negative)],
            probe_env,
        )

        verify_negative_control(probe, negative, interposer, probe_env)

        print("Control: bundled library alone", flush=True)
        run_command([str(probe), str(library)], probe_env)
        print("Regression: conflicting function loaded globally first", flush=True)
        run_command([str(probe), str(library), str(interposer)], probe_env)


if __name__ == "__main__":
    try:
        main()
    except RuntimeError as error:
        raise SystemExit(str(error))
