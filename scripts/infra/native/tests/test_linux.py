import pytest

from artifact_checks import (
    assert_dependencies,
    assert_glibc_ceiling,
    assert_harfbuzz_exports,
    assert_no_preemptible_harfbuzz_relocations,
    parse_dynamic_symbols,
    parse_linux_dependencies,
    run_tool,
)


pytestmark = pytest.mark.linux


def test_linux_dependencies(linux_dependencies, options):
    needed = parse_linux_dependencies(linux_dependencies)
    assert_dependencies(needed, options("--excluded"), options("--included"))


def test_linux_glibc_ceiling(linux_dependencies, pytestconfig):
    maximum = pytestconfig.getoption("--max-glibc")
    if maximum:
        assert_glibc_ceiling(linux_dependencies, maximum)


@pytest.fixture(scope="module")
def symbols(library, readelf):
    output = run_tool([readelf, "--wide", "--dyn-syms", library]).stdout
    return parse_dynamic_symbols(output)


@pytest.mark.harfbuzz
def test_harfbuzz_exported_api(symbols):
    assert_harfbuzz_exports(symbols)


@pytest.mark.harfbuzz
def test_harfbuzz_internal_function_relocations(library, readelf, symbols):
    output = run_tool([readelf, "--wide", "--relocs", library]).stdout
    assert_no_preemptible_harfbuzz_relocations(output, symbols)
