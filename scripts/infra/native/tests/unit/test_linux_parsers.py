import pytest

from artifact_checks import (
    ESSENTIAL_HARFBUZZ_APIS,
    Symbol,
    assert_dependencies,
    assert_glibc_ceiling,
    assert_harfbuzz_exports,
    assert_no_preemptible_harfbuzz_relocations,
    parse_dynamic_symbols,
    parse_linux_dependencies,
)


DEPENDENCIES = """
Dynamic section at offset 0x2de0 contains 5 entries:
  Tag        Type                         Name/Value
 0x00000001 (NEEDED)                     Shared library: [libc.so.6]
 0x00000001 (NEEDED)                     Shared library: [LIBpthread.so.0]
Version needs section '.gnu.version_r' contains 1 entry:
  0x0010: Name: GLIBC_2.17  Flags: none  Version: 3
"""


def symbol_table(wide=True, versioned=False):
    rows = ["Symbol table '.dynsym' contains 8 entries:",
            "   Num:    Value          Size Type    Bind   Vis      Ndx Name",
            "     0: 0000000000000000     0 NOTYPE  LOCAL  DEFAULT  UND"]
    value = "0000000000001100" if wide else "00001100"
    for index, name in enumerate(sorted(ESSENTIAL_HARFBUZZ_APIS), 1):
        suffix = "@@libHarfBuzzSharp" if versioned else ""
        rows.append(f" {index}: {value} 32 FUNC GLOBAL DEFAULT 12 {name}{suffix}")
    rows += [
        " 5: 00000000 4 OBJECT GLOBAL DEFAULT 22 hb_data",
        " 6: 00000000 0 FUNC GLOBAL DEFAULT UND hb_external@HB_SYSTEM (2)",
        " 7: 00000000 0 OBJECT GLOBAL DEFAULT ABS libHarfBuzzSharp",
    ]
    return "\n".join(rows)


def test_dependencies_preserve_case_insensitive_substring_semantics():
    needed = parse_linux_dependencies(DEPENDENCIES)
    assert needed == ["libc.so.6", "LIBpthread.so.0"]
    assert_dependencies(needed, excluded=["libstdc++"], included=[" LIBC.SO ", "pthread"])
    with pytest.raises(AssertionError, match="Forbidden dependency"):
        assert_dependencies(needed, excluded=["libPTHREAD"])
    with pytest.raises(AssertionError, match="Missing required dependency"):
        assert_dependencies(needed, included=["libm.so"])


@pytest.mark.parametrize("output", ["", "not an ELF", "There is no dynamic section in this file."])
def test_reject_unrecognized_dynamic_output(output):
    with pytest.raises(AssertionError, match="dynamic section"):
        parse_linux_dependencies(output)


def test_reject_malformed_needed():
    with pytest.raises(AssertionError, match="Malformed.*NEEDED"):
        parse_linux_dependencies("Dynamic section\n 1 (NEEDED) libc.so")


def test_no_needed_entries_can_be_valid():
    assert parse_linux_dependencies("Dynamic section at offset 0x10 contains 1 entry:") == []


def test_all_glibc_versions_checked_numerically():
    assert_glibc_ceiling(DEPENDENCIES + " GLIBC_2.9 GLIBC_2.27 GLIBC_2.17", "2.27")
    with pytest.raises(AssertionError, match="GLIBC_2.28 exceeds"):
        assert_glibc_ceiling(DEPENDENCIES + " GLIBC_2.28", "2.27")
    with pytest.raises(AssertionError, match="GLIBC_2.27.0 exceeds"):
        assert_glibc_ceiling("GLIBC_2.27.0", "2.27")


@pytest.mark.parametrize("version", ["PRIVATE", "2", "2..27", "2.27.x", "", "2.27-rc", "2.27+rc", "2.1.2.3.4", "2147483648.0"])
def test_malformed_glibc_versions_fail_clearly(version):
    with pytest.raises(AssertionError, match="Malformed GLIBC version"):
        assert_glibc_ceiling("GLIBC_" + version, "2.27")


@pytest.mark.parametrize("maximum", ["2", "latest", "2..27"])
def test_malformed_ceiling_fails(maximum):
    with pytest.raises(AssertionError, match="Malformed GLIBC version"):
        assert_glibc_ceiling("", maximum)


@pytest.mark.parametrize("wide", [True, False])
@pytest.mark.parametrize("versioned", [True, False])
def test_parse_gnu_32_and_64_bit_versioned_symbols(wide, versioned):
    symbols = parse_dynamic_symbols(symbol_table(wide, versioned))
    assert symbols[0].name == ""
    assert symbols[6].base_name == "hb_external"
    assert not symbols[6].defined_function
    assert_harfbuzz_exports(symbols)


@pytest.mark.parametrize("output", ["", "Symbol table '.dynsym' contains 0 entries:", "Symbol table '.symtab' contains 8 entries:"])
def test_empty_or_non_dynamic_symbols_fail(output):
    with pytest.raises(AssertionError):
        parse_dynamic_symbols(output)


def test_malformed_symbol_row_fails():
    with pytest.raises(AssertionError, match="Malformed readelf dynamic symbol"):
        parse_dynamic_symbols("Symbol table '.dynsym' contains 1 entry:\n 1: broken")


@pytest.mark.parametrize("name", ["malloc", "_ZCppSymbol", "libHarfBuzzSharp_extra"])
def test_forbidden_exports_fail(name):
    symbols = parse_dynamic_symbols(symbol_table())
    symbols[8] = Symbol(8, "FUNC", "GLOBAL", "DEFAULT", "12", name)
    with pytest.raises(AssertionError, match="Unexpected externally exported symbol"):
        assert_harfbuzz_exports(symbols)


def test_version_node_exception_requires_absolute_symbol():
    symbols = parse_dynamic_symbols(symbol_table())
    symbols[7] = Symbol(7, "FUNC", "GLOBAL", "DEFAULT", "12", "libHarfBuzzSharp")
    with pytest.raises(AssertionError, match="Unexpected externally exported symbol"):
        assert_harfbuzz_exports(symbols)


@pytest.mark.parametrize("kind,binding,visibility,section", [
    ("OBJECT", "GLOBAL", "DEFAULT", "12"),
    ("FUNC", "LOCAL", "DEFAULT", "12"),
    ("FUNC", "GLOBAL", "HIDDEN", "12"),
    ("FUNC", "GLOBAL", "DEFAULT", "UND"),
])
def test_essential_apis_must_be_exported_functions(kind, binding, visibility, section):
    symbols = parse_dynamic_symbols(symbol_table())
    symbols[1] = Symbol(1, kind, binding, visibility, section, symbols[1].name)
    with pytest.raises(AssertionError, match="Missing exported HarfBuzz functions"):
        assert_harfbuzz_exports(symbols)


@pytest.mark.parametrize("info,kind", [
    ("0000000100000007", "R_X86_64_JUMP_SLOT"),
    ("0000000100000006", "R_X86_64_GLOB_DAT"),
    ("0000000100000001", "R_X86_64_64"),
    ("0000000100000402", "R_AARCH64_JUMP_SLOT"),
    ("0000000100000101", "R_AARCH64_ABS64"),
    ("00000116", "R_ARM_JUMP_SLOT"),
    ("00000102", "R_ARM_ABS32"),
    ("00000107", "R_386_JUMP_SLOT"),
    ("0000000100000002", "R_RISCV_64"),
])
def test_any_relocation_to_defined_hb_function_fails(info, kind):
    symbols = parse_dynamic_symbols(symbol_table(versioned=True))
    output = f"Relocation section '.rela.dyn' at offset 0x100 contains 1 entry:\n 00002000 {info} {kind} 00001100 {symbols[1].name} + 0"
    with pytest.raises(AssertionError, match="Preemptible relocation"):
        assert_no_preemptible_harfbuzz_relocations(output, symbols)


def test_data_undefined_external_and_relative_relocations_are_allowed():
    symbols = parse_dynamic_symbols(symbol_table())
    output = """
Relocation section '.rela.dyn' at offset 0x100 contains 3 entries:
 00002000 0000000500000006 R_X86_64_GLOB_DAT 00001100 hb_data + 0
 00002008 0000000600000007 R_X86_64_JUMP_SLOT 00000000 hb_external@HB_SYSTEM + 0
 00002010 0000000000000008 R_X86_64_RELATIVE 1100
"""
    assert_no_preemptible_harfbuzz_relocations(output, symbols)


def test_ifunc_is_a_function_even_when_protected():
    symbols = {0: Symbol(0, "IFUNC", "WEAK", "PROTECTED", "12", "hb_func")}
    with pytest.raises(AssertionError, match="defined HarfBuzz function"):
        assert_no_preemptible_harfbuzz_relocations(
            "Relocation section '.rela.dyn'\n 00002000 0000000000000001 R_X86_64_64 0000 hb_func + 0",
            symbols,
        )


def test_unknown_relocation_symbol_fails():
    with pytest.raises(AssertionError, match="missing dynamic symbol"):
        assert_no_preemptible_harfbuzz_relocations(
            "Relocation section '.rela.dyn'\n 00002000 0000009900000001 R_X86_64_64 0000 hb_func + 0",
            parse_dynamic_symbols(symbol_table()),
        )


def test_no_relocations_is_recognized_but_empty_output_fails():
    symbols = parse_dynamic_symbols(symbol_table())
    assert_no_preemptible_harfbuzz_relocations("There are no relocations in this file.", symbols)
    with pytest.raises(AssertionError, match="relocation information"):
        assert_no_preemptible_harfbuzz_relocations("", symbols)
