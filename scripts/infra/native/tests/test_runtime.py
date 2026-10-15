from pathlib import Path
import sys

import pytest

from artifact_checks import assert_runtime_host, run_tool
from native_process import RUNTIME_TIMEOUT


pytestmark = pytest.mark.runtime


def test_harfbuzz_symbol_binding_runtime(library, pytestconfig):
    assert_runtime_host(
        library, pytestconfig.getoption("--libc"), pytestconfig.getoption("--readelf")
    )
    script = Path(__file__).parents[1] / "linux" / "test-harfbuzz-symbol-binding.py"
    result = run_tool(
        [sys.executable, script, library, "--cc", pytestconfig.getoption("--cc"),
         "--inherited-process-group"],
        timeout=RUNTIME_TIMEOUT,
    )
    assert "PASS: negative control was interposed (exit 86)" in result.stdout, (
        f"Runtime harness did not verify its negative control:\n{result.stdout}"
    )
    assert result.stdout.count("PASS: bundled font creation and OpenType functions") == 3, (
        f"Runtime harness did not complete control and actual-artifact probes:\n{result.stdout}"
    )
