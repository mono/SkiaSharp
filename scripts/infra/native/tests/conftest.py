import os
from pathlib import Path

import pytest

from artifact_checks import gnu_readelf, require_file, require_tool, run_tool


def pytest_addoption(parser):
    group = parser.getgroup("native artifacts")
    group.addoption("--lane", choices=("linux", "windows", "runtime"))
    group.addoption("--library")
    group.addoption("--excluded", default="")
    group.addoption("--included", default="")
    group.addoption("--max-glibc")
    group.addoption("--delay-loaded", default="")
    group.addoption("--dumpbin", default="dumpbin")
    group.addoption("--readelf", default=os.environ.get("READELF", "readelf"))
    group.addoption("--check-runtime-marker", choices=("true", "false"), default="true")
    group.addoption("--libc", choices=("glibc", "musl"))
    group.addoption("--cc", default=os.environ.get("CC", "cc"))


def unit_invocation(config):
    unit = Path(__file__).parent / "unit"
    return bool(config.args) and all(
        (Path(str(arg).split("::", 1)[0]).resolve() == unit
         or unit in Path(str(arg).split("::", 1)[0]).resolve().parents)
        for arg in config.args
    )


def pytest_configure(config):
    for lane in ("linux", "windows", "runtime"):
        config.addinivalue_line("markers", f"{lane}: native artifact {lane} lane")
    config.addinivalue_line("markers", "harfbuzz: HarfBuzz-only artifact inspection")
    config.addinivalue_line("markers", "runtime_marker: Windows PDB runtime marker")
    if not unit_invocation(config):
        if not config.getoption("--lane"):
            raise pytest.UsageError("--lane=linux|windows|runtime is required")
        if not config.getoption("--library"):
            raise pytest.UsageError("--library=/absolute/file is required")
        if config.getoption("--lane") == "runtime" and not config.getoption("--libc"):
            raise pytest.UsageError("--libc=glibc|musl is required for the runtime lane")


def pytest_ignore_collect(collection_path, config):
    if collection_path.name == "unit" and not unit_invocation(config):
        return True
    return None


def pytest_collection_modifyitems(config, items):
    if unit_invocation(config):
        return
    lane = config.getoption("--lane")
    harfbuzz = "harfbuzzsharp" in Path(config.getoption("--library")).name.casefold()
    selected, deselected = [], []
    for item in items:
        keep = item.get_closest_marker(lane) is not None
        keep = keep and (not item.get_closest_marker("harfbuzz") or harfbuzz)
        keep = keep and (
            not item.get_closest_marker("runtime_marker")
            or config.getoption("--check-runtime-marker") == "true"
        )
        (selected if keep else deselected).append(item)
    items[:] = selected
    config.hook.pytest_deselected(items=deselected)
    if not selected:
        raise pytest.UsageError(f"No artifact tests selected for lane {lane!r}")


@pytest.fixture(scope="session")
def library(pytestconfig):
    return require_file(pytestconfig.getoption("--library"))


@pytest.fixture(scope="session")
def readelf(pytestconfig):
    return gnu_readelf(pytestconfig.getoption("--readelf"))


@pytest.fixture(scope="session")
def linux_dependencies(library, readelf):
    return run_tool([readelf, "--wide", "-dV", library]).stdout


@pytest.fixture(scope="session")
def windows_dependencies(library, pytestconfig):
    dumpbin = require_tool(pytestconfig.getoption("--dumpbin"))
    return run_tool([dumpbin, "/dependents", library]).stdout


@pytest.fixture
def options(pytestconfig):
    def values(name):
        return [value.strip() for value in pytestconfig.getoption(name).split(",") if value.strip()]
    return values
