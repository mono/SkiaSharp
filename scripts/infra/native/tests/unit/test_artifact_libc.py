import subprocess

import pytest

import artifact_checks as checks
from test_runtime_and_tools import elf_header


GLIBC_REQUIREMENTS = """
Version needs section '.gnu.version_r' contains 1 entry:
 Addr: 0x0000000000001000  Offset: 0x001000  Link: 4 (.dynstr)
  0x000000: Version: 1  File: libc.so.6  Cnt: 1
  0x000010:   Name: GLIBC_2.17  Flags: none  Version: 2
"""


def dynamic_output(*dependencies, versions=""):
    return (
        "Dynamic section at offset 0x100 contains 5 entries:\n"
        + "".join(f" 1 (NEEDED) Shared library: [{name}]\n" for name in dependencies)
        + versions
    )


@pytest.mark.parametrize("output,expected", [
    (dynamic_output("libc.so.6", versions=GLIBC_REQUIREMENTS), "glibc"),
    (dynamic_output("libc.so.6"), "glibc"),
    (dynamic_output("libc.musl-x86_64.so.1"), "musl"),
    (dynamic_output("libc.musl-aarch64.so.1", "libm.so"), "musl"),
])
def test_artifact_libc_is_derived_from_dynamic_requirements(output, expected):
    assert checks.parse_artifact_libc(output) == expected


@pytest.mark.parametrize("output,diagnostic", [
    (dynamic_output(), "Unknown artifact libc"),
    (dynamic_output("libc.so"), "Unknown artifact libc"),
    (dynamic_output("libc.musl-.so.1"), "Unknown artifact libc"),
    (dynamic_output("libc.so.6", "libc.musl-x86_64.so.1"), "mixed glibc/musl"),
    (dynamic_output("libc.musl-x86_64.so.1", versions=GLIBC_REQUIREMENTS), "mixed glibc/musl"),
    (dynamic_output(versions=GLIBC_REQUIREMENTS), "Unknown artifact libc"),
])
def test_unknown_or_mixed_artifact_libc_is_rejected(output, diagnostic):
    with pytest.raises(AssertionError, match=diagnostic):
        checks.parse_artifact_libc(output)


@pytest.mark.parametrize("host,artifact,dependencies,versions", [
    ("musl", "glibc", ["libc.so.6"], GLIBC_REQUIREMENTS),
    ("glibc", "musl", ["libc.musl-x86_64.so.1"], ""),
])
def test_artifact_libc_mismatch_fails_even_when_host_matches_request(
    monkeypatch, workdir, host, artifact, dependencies, versions
):
    library = workdir / "artifact.so"
    library.write_bytes(elf_header())
    monkeypatch.setattr(checks.platform, "system", lambda: "Linux")
    monkeypatch.setattr(checks.platform, "machine", lambda: "x86_64")
    monkeypatch.setattr(checks, "detect_host_libc", lambda: host)
    monkeypatch.setattr(checks, "gnu_readelf", lambda command: "/gnu/readelf")
    calls = []
    def readelf(command):
        calls.append(command)
        return subprocess.CompletedProcess(
            command, 0, dynamic_output(*dependencies, versions=versions), ""
        )
    monkeypatch.setattr(checks, "run_tool", readelf)
    with pytest.raises(AssertionError, match=f"Artifact libc {artifact} does not match requested libc {host}"):
        checks.assert_runtime_host(library, host, "/gnu/readelf")
    assert calls == [["/gnu/readelf", "--wide", "-dV", library]]


def test_alpine_aarch64_libgcc_glibc_abi_version_does_not_imply_glibc_libc():
    versions = """
Version symbols section '.gnu.version' contains 6 entries:
 Addr: 0x0000000000001000  Offset: 0x001000  Link: 3 (.dynsym)
  000:   0 (*local*)       2 (GLIBC_2.0)     3 (GCC_3.0)

Version needs section '.gnu.version_r' contains 2 entries:
 Addr: 0x0000000000001100  Offset: 0x001100  Link: 4 (.dynstr)
  0x000000: Version: 1  File: libgcc_s.so.1  Cnt: 2
  0x000010:   Name: GCC_3.0  Flags: none  Version: 3
  0x000020:   Name: GLIBC_2.0  Flags: none  Version: 2
  0x000030: Version: 1  File: libstdc++.so.6  Cnt: 1
  0x000040:   Name: CXXABI_1.3  Flags: none  Version: 4
"""
    output = dynamic_output(
        "libstdc++.so.6", "libgcc_s.so.1", "libc.musl-aarch64.so.1", versions=versions
    )
    assert checks.parse_version_requirements(output) == {
        "libgcc_s.so.1": ["GCC_3.0", "GLIBC_2.0"],
        "libstdc++.so.6": ["CXXABI_1.3"],
    }
    assert checks.parse_artifact_libc(output) == "musl"


def test_glibc_version_definition_is_not_a_libc_provider_requirement():
    output = dynamic_output(
        "libc.musl-aarch64.so.1",
        versions="""
Version definition section '.gnu.version_d' contains 1 entry:
  0x000000: Rev: 1  Flags: BASE  Index: 1  Cnt: 1  Name: GLIBC_2.0
""",
    )
    assert checks.parse_version_requirements(output) == {}
    assert checks.parse_artifact_libc(output) == "musl"
