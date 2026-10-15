from pathlib import Path
import os
import subprocess
import sys
import xml.etree.ElementTree as ET

import pytest


SUITE = Path(__file__).parents[1]


def invoke(*arguments):
    env = os.environ.copy()
    env["LC_ALL"] = "C"
    return subprocess.run(
        [sys.executable, "-m", "pytest", str(SUITE), "-p", "no:cacheprovider", *map(str, arguments)],
        capture_output=True, text=True, timeout=60, env=env,
    )


@pytest.mark.parametrize("arguments,message", [
    ([], "--lane=linux|windows|runtime is required"),
    (["--lane=other"], "invalid choice"),
    (["--lane="], "invalid choice"),
    (["--lane=linux"], "--library=/absolute/file is required"),
    (["--lane=runtime", "--library=/missing.so"], "--libc=glibc|musl is required"),
])
def test_invalid_invocations_fail(arguments, message):
    result = invoke(*arguments)
    assert result.returncode != 0
    assert message in result.stdout + result.stderr


@pytest.mark.parametrize("lane,library,extra,expected", [
    ("linux", "libSkiaSharp.so", [], 2),
    ("linux", "libHarfBuzzSharp.so", [], 4),
    ("windows", "libHarfBuzzSharp.dll", [], 3),
    ("windows", "libHarfBuzzSharp.dll", ["--check-runtime-marker=false"], 2),
    ("runtime", "libHarfBuzzSharp.so", ["--libc=glibc"], 1),
])
def test_only_selected_lane_is_collected(lane, library, extra, expected, workdir):
    result = invoke("--collect-only", "-q", f"--lane={lane}", f"--library={workdir / library}", *extra)
    assert result.returncode == 0, result.stdout + result.stderr
    assert f"{expected}/" in result.stdout
    assert "::test_" in result.stdout
    assert "/unit/" not in result.stdout
    assert " skipped" not in result.stdout


def test_missing_artifact_is_failure_in_standard_junit(workdir):
    report = workdir / "missing-artifact.xml"
    result = invoke("--lane=linux", f"--library={workdir / 'missing.so'}", "-q", f"--junitxml={report}")
    assert result.returncode != 0
    root = ET.parse(report).getroot()
    cases = root.findall(".//testcase")
    assert len(cases) == 2
    assert all(case.find("error") is not None for case in cases)
    assert not root.findall(".//skipped")
    assert "Artifact does not exist" in result.stdout


def test_missing_inspection_tool_is_failure(workdir):
    artifact = workdir / "libSkiaSharp.so"
    artifact.write_bytes(b"ELF fixture")
    result = invoke("--lane=linux", f"--library={artifact}", "--readelf=/missing/readelf", "-q")
    assert result.returncode != 0
    assert "Required tool not found" in result.stdout
    assert " skipped" not in result.stdout


def fake_tool(workdir, name, outputs):
    path = workdir / name
    path.write_text(
        f"#!{sys.executable}\n"
        "import sys\n"
        f"outputs = {outputs!r}\n"
        "key = next((key for key in outputs if key in sys.argv[1:]), None)\n"
        "if key is None:\n"
        "    raise SystemExit(7)\n"
        "print(outputs[key])\n",
        encoding="utf-8",
    )
    path.chmod(0o755)
    return path


def test_cross_host_linux_inspection_success_uses_standard_junit(workdir):
    library = workdir / "libHarfBuzzSharp.so"
    # Deliberately non-loadable: inspection must delegate only to the selected tool.
    library.write_bytes(b"controlled cross-host artifact")
    symbols = """Symbol table '.dynsym' contains 5 entries:
 0: 0000000000000000 0 NOTYPE LOCAL DEFAULT UND
 1: 0000000000001100 4 FUNC GLOBAL DEFAULT 12 hb_face_get_empty@@libHarfBuzzSharp
 2: 0000000000001110 4 FUNC GLOBAL DEFAULT 12 hb_font_create@@libHarfBuzzSharp
 3: 0000000000001120 4 FUNC GLOBAL DEFAULT 12 hb_font_destroy@@libHarfBuzzSharp
 4: 0000000000001130 4 FUNC GLOBAL DEFAULT 12 hb_ot_font_set_funcs@@libHarfBuzzSharp
"""
    readelf = fake_tool(workdir, "readelf-fixture", {
        "--version": "GNU readelf (GNU Binutils) 2.40",
        "-dV": "Dynamic section at offset 0x100 contains 1 entry:\n 1 (NEEDED) Shared library: [libc.so.6]\n GLIBC_2.17",
        "--dyn-syms": symbols,
        "--relocs": "There are no relocations in this file.",
    })
    report = workdir / "linux.xml"
    result = invoke(
        "--lane=linux", f"--library={library}", f"--readelf={readelf}",
        "--included=LIBC", "--excluded=libstdc++", "--max-glibc=2.27",
        "-q", f"--junitxml={report}",
    )
    assert result.returncode == 0, result.stdout + result.stderr
    cases = ET.parse(report).getroot().findall(".//testcase")
    assert len(cases) == 4
    assert all(not list(case) for case in cases)


def test_cross_host_windows_checks_hard_delay_and_pdb(workdir):
    library = workdir / "libHarfBuzzSharp.dll"
    library.write_bytes(b"controlled cross-host artifact")
    library.with_suffix(".pdb").write_bytes(b"\0OBJR_SPECTRE\0")
    dumpbin = fake_tool(workdir, "dumpbin-fixture", {
        "/dependents": """Image has the following dependencies:
    kernel32.dll
Image has the following delay load dependencies:
    dwrite.dll
Summary
""",
    })
    result = invoke(
        "--lane=windows", f"--library={library}", f"--dumpbin={dumpbin}",
        "--included=kernel", "--excluded=dwrite", "--delay-loaded=DWRITE", "-q",
    )
    assert result.returncode == 0, result.stdout + result.stderr
    assert "3 passed" in result.stdout
    library.with_suffix(".pdb").unlink()
    result = invoke(
        "--lane=windows", f"--library={library}", f"--dumpbin={dumpbin}", "-q",
    )
    assert result.returncode != 0
    assert "Artifact does not exist" in result.stdout


def test_missing_windows_tool_is_failure(workdir):
    library = workdir / "libHarfBuzzSharp.dll"
    library.write_bytes(b"controlled artifact")
    result = invoke(
        "--lane=windows", f"--library={library}",
        "--dumpbin=/missing/dumpbin", "--check-runtime-marker=false", "-q",
    )
    assert result.returncode != 0
    assert "Required tool not found" in result.stdout
