"""Cross-host artifact inspection; native code runs only in the runtime lane."""

from dataclasses import dataclass
import os
from pathlib import Path
import platform
import re
import shutil
import struct
import subprocess

from native_process import COMMAND_TIMEOUT, run_process


TIMEOUT = COMMAND_TIMEOUT
ESSENTIAL_HARFBUZZ_APIS = {
    "hb_face_get_empty",
    "hb_font_create",
    "hb_font_destroy",
    "hb_ot_font_set_funcs",
}


def require_file(path):
    artifact = Path(path)
    assert artifact.is_absolute(), f"Artifact path must be absolute: {artifact}"
    assert artifact.is_file(), f"Artifact does not exist or is not a file: {artifact}"
    assert artifact.stat().st_size, f"Artifact is empty: {artifact}"
    return artifact


def require_tool(command):
    executable = shutil.which(command)
    assert executable, f"Required tool not found or not executable: {command}"
    return executable


def run_tool(command, timeout=TIMEOUT, check=True):
    env = os.environ.copy()
    env["LC_ALL"] = "C"
    try:
        result = run_process(
            [str(arg) for arg in command],
            env=env,
            timeout=timeout,
        )
    except subprocess.TimeoutExpired as error:
        raise AssertionError(
            f"Command timed out after {timeout}s: {command!r}\n"
            f"stdout: {error.stdout!r}\nstderr: {error.stderr!r}"
        ) from error
    except OSError as error:
        raise AssertionError(f"Could not execute {command!r}: {error}") from error
    assert not check or result.returncode == 0, (
        f"Command failed ({result.returncode}): {command!r}\n"
        f"stdout:\n{result.stdout}\nstderr:\n{result.stderr}"
    )
    return result


def gnu_readelf(command):
    executable = require_tool(command)
    version = run_tool([executable, "--version"]).stdout
    assert "GNU readelf" in version, f"GNU readelf is required, got:\n{version}"
    return executable


def parse_linux_dependencies(output):
    assert "Dynamic section" in output, "readelf did not report an ELF dynamic section"
    needed = []
    for line in output.splitlines():
        if "(NEEDED)" in line:
            match = re.search(r"\(NEEDED\).+\[([^\]]+)\]", line)
            assert match, f"Malformed readelf NEEDED entry: {line}"
            needed.append(match.group(1))
    return needed


def assert_dependencies(needed, excluded=(), included=()):
    folded = [name.casefold() for name in needed]
    for name in excluded:
        assert not any(name.strip().casefold() in dep for dep in folded), (
            f"Forbidden dependency {name!r}; dependencies: {needed}"
        )
    for name in included:
        assert any(name.strip().casefold() in dep for dep in folded), (
            f"Missing required dependency {name!r}; dependencies: {needed}"
        )


def parse_version(value):
    assert re.fullmatch(r"[0-9]+(?:\.[0-9]+){1,3}", value), (
        f"Malformed GLIBC version: {value!r}; expected two to four numeric components"
    )
    version = tuple(int(part) for part in value.split("."))
    assert all(part <= 2147483647 for part in version), (
        f"Malformed GLIBC version: {value!r}; component exceeds Int32"
    )
    return version


def assert_glibc_ceiling(output, maximum):
    ceiling = parse_version(maximum)
    for version in sorted(set(re.findall(r"""\bGLIBC_([^\s()[\]@'"<>:;,]*)""", output))):
        assert parse_version(version) <= ceiling, (
            f"GLIBC_{version} exceeds maximum GLIBC_{maximum}"
        )


@dataclass(frozen=True)
class Symbol:
    index: int
    kind: str
    binding: str
    visibility: str
    section: str
    name: str

    @property
    def base_name(self):
        return self.name.split("@", 1)[0]

    @property
    def exported(self):
        return (
            self.section != "UND"
            and self.binding in {"GLOBAL", "WEAK", "UNIQUE"}
            and self.visibility in {"DEFAULT", "PROTECTED"}
        )

    @property
    def defined_function(self):
        return self.section != "UND" and self.kind in {"FUNC", "IFUNC"}


def parse_dynamic_symbols(output):
    assert re.search(r"Symbol table ['\"]\.dynsym['\"]", output), (
        "readelf did not report a .dynsym symbol table"
    )
    symbols = {}
    for line in output.splitlines():
        match = re.match(
            r"\s*(\d+):\s+[0-9a-fA-F]+\s+\S+\s+(\S+)\s+(\S+)\s+"
            r"(\S+)\s+(\S+)(?:\s+(\S+))?", line
        )
        if match:
            index, kind, binding, visibility, section, name = match.groups()
            symbol = Symbol(int(index), kind, binding, visibility, section, name or "")
            symbols[symbol.index] = symbol
        elif re.match(r"\s*\d+:", line):
            raise AssertionError(f"Malformed readelf dynamic symbol: {line}")
    assert symbols, "ELF dynamic symbol table is empty or unreadable"
    return symbols


def assert_harfbuzz_exports(symbols):
    exports = [symbol for symbol in symbols.values() if symbol.exported]
    assert exports, "HarfBuzz artifact has no externally exported symbols"
    for symbol in exports:
        assert symbol.base_name.startswith("hb_") or (
            symbol.name == "libHarfBuzzSharp" and symbol.section == "ABS"
        ), f"Unexpected externally exported symbol: {symbol.name}"
    functions = {s.base_name for s in exports if s.defined_function}
    missing = ESSENTIAL_HARFBUZZ_APIS - functions
    assert not missing, f"Missing exported HarfBuzz functions: {sorted(missing)}"


def assert_no_preemptible_harfbuzz_relocations(output, symbols):
    assert (
        re.search(r"Relocation section ['\"]", output)
        or "There are no relocations in this file." in output
    ), "readelf did not report relocation information"
    for line in output.splitlines():
        match = re.match(
            r"\s*([0-9a-fA-F]+)\s+([0-9a-fA-F]+)\s+(R_\S+)(?:\s+.*)?$", line
        )
        if not match:
            if re.match(r"\s*[0-9a-fA-F]+\s+[0-9a-fA-F]+\s+R_", line):
                raise AssertionError(f"Malformed readelf relocation: {line}")
            continue
        _, info, kind = match.groups()
        # ELF64 encodes the symbol index in the upper 32 bits; ELF32 uses 24.
        index = int(info, 16) >> (32 if len(info) > 8 else 8)
        assert index in symbols, f"Relocation references missing dynamic symbol {index}: {line}"
        symbol = symbols[index]
        assert not (
            symbol.base_name.startswith("hb_") and symbol.defined_function
        ), f"Preemptible relocation {kind} references defined HarfBuzz function {symbol.name}: {line}"


def parse_windows_dependencies(output):
    needed, delayed = [], []
    current = None
    recognized = False
    for line in output.splitlines():
        lower = line.casefold()
        if "has the following dependencies:" in lower:
            current = needed
            recognized = True
        elif "has the following delay load dependencies:" in lower:
            current = delayed
            recognized = True
        elif re.match(r"\s*summary\b", lower):
            current = None
        elif current is not None:
            match = re.fullmatch(r"\s{2,}(\S+\.dll)\s*", line, re.IGNORECASE)
            if match:
                current.append(match.group(1))
    assert recognized, "dumpbin did not report dependency sections"
    return needed, delayed


def assert_delay_loaded(needed, delayed, required):
    for name in required:
        assert not any(name.strip().casefold() in dep.casefold() for dep in needed), (
            f"{name!r} is a hard dependency but must be delay-loaded"
        )
    assert_dependencies(delayed, included=required)


def assert_windows_runtime_marker(contents):
    lowered = contents.lower()
    assert b"objr_spectre" in lowered or b"objd_spectre" in lowered, (
        "PDB has no Spectre runtime build marker (objr_spectre or objd_spectre)"
    )


def read_elf_machine(library):
    with library.open("rb") as stream:
        header = stream.read(20)
    assert len(header) == 20 and header[:4] == b"\x7fELF", "Runtime artifact is not ELF"
    assert header[4] in (1, 2), f"Unknown ELF class: {header[4]}"
    assert header[5] in (1, 2), f"Unknown ELF byte order: {header[5]}"
    machine = struct.unpack("<H" if header[5] == 1 else ">H", header[18:20])[0]
    return machine, header[4], header[5]


def detect_host_libc():
    description = " ".join(platform.libc_ver()).casefold()
    if "glibc" in description:
        return "glibc"
    if "musl" in description:
        return "musl"
    ldd = require_tool("ldd")
    result = run_tool([ldd, "--version"], check=False)
    description = (result.stdout + result.stderr).casefold()
    if "musl" in description:
        return "musl"
    if "glibc" in description or "gnu libc" in description or "gnu c library" in description:
        return "glibc"
    raise AssertionError(f"Could not identify host libc from ldd:\n{description}")


def parse_version_requirements(output):
    requirements = {}
    provider = None
    in_needs = False
    for line in output.splitlines():
        if line.strip().startswith("Version needs section "):
            in_needs = True
            provider = None
            continue
        if line and not line[0].isspace():
            in_needs = False
            provider = None
        if not in_needs:
            continue
        file_match = re.search(r"\bFile:\s+(\S+)", line)
        if file_match:
            provider = file_match.group(1)
            requirements.setdefault(provider, [])
        name_match = re.search(r"\bName:\s+(\S+)", line)
        if name_match:
            assert provider is not None, f"Version requirement has no provider: {line}"
            requirements[provider].append(name_match.group(1))
    return requirements


def parse_artifact_libc(output):
    needed = parse_linux_dependencies(output)
    glibc = "libc.so.6" in needed
    musl = any(re.fullmatch(r"libc\.musl-[^/]+\.so\.1", name) for name in needed)
    requirements = parse_version_requirements(output)
    glibc_versions = [
        name for name in requirements.get("libc.so.6", ()) if name.startswith("GLIBC_")
    ]
    assert not (musl and (glibc or glibc_versions)), (
        f"Artifact has mixed glibc/musl requirements; dependencies: {needed}; "
        f"libc.so.6 version requirements: {glibc_versions}"
    )
    if glibc:
        return "glibc"
    if musl:
        return "musl"
    raise AssertionError(
        f"Unknown artifact libc: expected libc.so.6 or libc.musl-*.so.1; "
        f"dependencies: {needed}; libc.so.6 version requirements: {glibc_versions}"
    )


def inspect_artifact_libc(library, readelf):
    executable = gnu_readelf(readelf)
    output = run_tool([executable, "--wide", "-dV", library]).stdout
    return parse_artifact_libc(output)


def assert_runtime_host(library, expected_libc, readelf="readelf"):
    assert platform.system() == "Linux", "Runtime lane requires a Linux host"
    architectures = {
        "x86_64": (62, 2, 1), "amd64": (62, 2, 1),
        "i386": (3, 1, 1), "i486": (3, 1, 1), "i586": (3, 1, 1), "i686": (3, 1, 1),
        "aarch64": (183, 2, 1), "arm64": (183, 2, 1),
        "armv7l": (40, 1, 1), "armv6l": (40, 1, 1),
        "riscv64": (243, 2, 1), "s390x": (22, 2, 2),
        "ppc64le": (21, 2, 1), "ppc64": (21, 2, 2),
        "loongarch64": (258, 2, 1),
    }
    host = platform.machine().casefold()
    assert host in architectures, f"Unsupported runtime host architecture: {host}"
    actual = read_elf_machine(library)
    assert actual == architectures[host], (
        f"Artifact ELF machine/class/endianness {actual} does not match host {host} "
        f"{architectures[host]}"
    )
    actual_libc = detect_host_libc()
    assert actual_libc == expected_libc, (
        f"Expected libc {expected_libc} does not match host libc {actual_libc}"
    )
    artifact_libc = inspect_artifact_libc(library, readelf)
    assert artifact_libc == expected_libc, (
        f"Artifact libc {artifact_libc} does not match requested libc {expected_libc} "
        f"and host libc {actual_libc}"
    )
