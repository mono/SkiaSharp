"""Adapt the pinned Chromium 6275 toolchain to v143 inside a VS 2026 IDE."""

import pathlib
import sys


def replace_once(path, original, replacement):
    contents = path.read_text(encoding="utf-8")
    if replacement in contents:
        return
    if contents.count(original) != 1:
        raise RuntimeError(f"Expected exactly one toolchain anchor in {path}: {original}")
    path.write_text(contents.replace(original, replacement), encoding="utf-8")


def configure(angle, visual_studio, version):
    compiler = visual_studio / "VC/Tools/MSVC" / version
    if not (compiler / "bin/Hostx64/arm64/cl.exe").is_file():
        raise RuntimeError(f"Missing selected v143 compiler: {compiler}")
    redist = visual_studio / "VC/Redist/MSVC"
    candidates = [
        path for path in redist.glob("14.*")
        if (path / "arm64/Microsoft.VC143.CRT/msvcp140.dll").is_file()
    ]
    if not candidates:
        raise RuntimeError(f"Missing Installer-provided ARM64 v143 CRT under {redist}")
    selected = max(candidates, key=lambda path: tuple(map(int, path.name.split("."))))
    print(f"ANGLE VS 2026: compiler {compiler}, ARM64 v143 CRT {selected}")

    replace_once(
        angle / "build/toolchain/win/setup_toolchain.py",
        "    args.append(SDK_VERSION)\n    variables = _LoadEnvFromBat(args)",
        "    args.append(SDK_VERSION)\n"
        f"    args.append('-vcvars_ver={version}')\n"
        "    variables = _LoadEnvFromBat(args)",
    )
    replace_once(
        angle / "build/vs_toolchain.py",
        "  return FindVCComponentRoot('Redist')",
        f"  return {str(selected)!r}",
    )


if __name__ == "__main__":
    configure(pathlib.Path(sys.argv[1]), pathlib.Path(sys.argv[2]), sys.argv[3])
