import pytest

from artifact_checks import (
    assert_delay_loaded,
    assert_dependencies,
    assert_windows_runtime_marker,
    parse_windows_dependencies,
)


DUMPBIN = """
Microsoft (R) COFF/PE Dumper Version 14.40
Dump of file C:\\artifacts\\libHarfBuzzSharp.dll
  Image has the following dependencies:

    KERNEL32.dll
    api-ms-win-crt-runtime-l1-1-0.dll

  Image has the following delay load dependencies:

    dwrite.dll
    D3D12.DLL

  Summary
       18000 .text
    ignored.dll
"""


def test_windows_hard_and_delay_sections_are_distinct():
    needed, delayed = parse_windows_dependencies(DUMPBIN)
    assert needed == ["KERNEL32.dll", "api-ms-win-crt-runtime-l1-1-0.dll"]
    assert delayed == ["dwrite.dll", "D3D12.DLL"]
    assert_dependencies(needed, excluded=["dwrite"], included=["KERNEL32", "CRT"])
    assert_delay_loaded(needed, delayed, ["DWRITE", "d3d12"])


def test_delay_dependency_does_not_satisfy_hard_requirement():
    needed, _ = parse_windows_dependencies(DUMPBIN)
    with pytest.raises(AssertionError, match="Missing required dependency"):
        assert_dependencies(needed, included=["dwrite"])


def test_delay_requirement_cannot_be_hard_dependency():
    with pytest.raises(AssertionError, match="hard dependency"):
        assert_delay_loaded(["DWrite.dll"], ["dwrite.dll"], ["dwrite"])


def test_delay_requirement_must_be_present():
    with pytest.raises(AssertionError, match="Missing required dependency"):
        assert_delay_loaded(["kernel32.dll"], [], ["dwrite"])


def test_no_delayed_section_is_valid_if_not_required():
    assert parse_windows_dependencies("  Image has the following dependencies:\n    kernel32.dll") == (["kernel32.dll"], [])


@pytest.mark.parametrize("output", ["", "error LNK1107: invalid or corrupt file", "Summary\n    kernel32.dll"])
def test_unrecognized_dumpbin_output_fails(output):
    with pytest.raises(AssertionError, match="dependency sections"):
        parse_windows_dependencies(output)


@pytest.mark.parametrize("contents", [b"\0ObjR_Spectre\0", b"\xffOBJD_SPECTRE\0"])
def test_pdb_runtime_marker_is_ascii_case_insensitive(contents):
    assert_windows_runtime_marker(contents)


@pytest.mark.parametrize("contents", [b"", b"spectre", b"objr", b"objr_normal"])
def test_missing_pdb_runtime_marker_fails(contents):
    with pytest.raises(AssertionError, match="runtime build marker"):
        assert_windows_runtime_marker(contents)
