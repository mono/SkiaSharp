import importlib.util
from pathlib import Path
import tempfile

import pytest


@pytest.fixture
def workdir():
    with tempfile.TemporaryDirectory(prefix=".native-artifact-unit-", dir=Path.cwd()) as directory:
        yield Path(directory)


@pytest.fixture
def harness():
    script = Path(__file__).parents[2] / "linux" / "test-harfbuzz-symbol-binding.py"
    spec = importlib.util.spec_from_file_location("harfbuzz_binding_probe", script)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module
