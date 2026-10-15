import pytest

from artifact_checks import (
    assert_delay_loaded,
    assert_dependencies,
    assert_windows_runtime_marker,
    parse_windows_dependencies,
    require_file,
)


pytestmark = pytest.mark.windows


def test_windows_hard_dependencies(windows_dependencies, options):
    needed, _ = parse_windows_dependencies(windows_dependencies)
    assert_dependencies(needed, options("--excluded"), options("--included"))


def test_windows_delay_loaded_dependencies(windows_dependencies, options):
    needed, delayed = parse_windows_dependencies(windows_dependencies)
    assert_delay_loaded(needed, delayed, options("--delay-loaded"))


@pytest.mark.runtime_marker
def test_windows_spectre_runtime_build_marker(library):
    pdb = require_file(library.with_suffix(".pdb"))
    assert_windows_runtime_marker(pdb.read_bytes())
