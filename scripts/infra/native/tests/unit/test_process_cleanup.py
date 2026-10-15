import errno
import importlib.util
import os
from pathlib import Path
import signal
import subprocess
import sys
import time

import pytest

import artifact_checks as checks
import native_process


def assert_process_stopped(pid):
    deadline = time.monotonic() + 3
    while time.monotonic() < deadline:
        try:
            os.kill(pid, 0)
        except OSError as error:
            if error.errno == errno.ESRCH:
                return
            raise
        # Linux container PID 1 may not promptly reap an adopted zombie.
        stat = Path(f"/proc/{pid}/stat")
        if stat.is_file():
            try:
                if stat.read_text().rsplit(")", 1)[1].split()[0] == "Z":
                    return
            except FileNotFoundError:
                return
        time.sleep(0.02)
    pytest.fail(f"Descendant {pid} survived subprocess cleanup")


@pytest.mark.parametrize("exit_parent", [False, True])
def test_timeout_and_early_parent_exit_kill_sleeping_descendants(workdir, exit_parent):
    pid_file = workdir / "descendant.pid"
    child_source = (
        "import signal,time;"
        "signal.signal(signal.SIGTERM, signal.SIG_IGN);"
        "time.sleep(120)"
    )
    source = (
        "import pathlib,subprocess,sys,time\n"
        f"child = subprocess.Popen([sys.executable, '-c', {child_source!r}])\n"
        f"pathlib.Path({str(pid_file)!r}).write_text(str(child.pid))\n"
        + ("" if exit_parent else "time.sleep(120)\n")
    )
    try:
        if exit_parent:
            result = native_process.run_process([sys.executable, "-c", source], timeout=2)
            assert result.returncode == 0
        else:
            with pytest.raises(subprocess.TimeoutExpired):
                native_process.run_process([sys.executable, "-c", source], timeout=0.5)
        assert pid_file.is_file()
        assert_process_stopped(int(pid_file.read_text()))
    finally:
        if pid_file.is_file():
            try:
                os.kill(int(pid_file.read_text()), signal.SIGKILL)
            except ProcessLookupError:
                pass


def test_nested_timeout_keeps_descendants_in_outer_group_and_cleans_scratch(workdir):
    pid_file = workdir / "descendant.pid"
    group_file = workdir / "group.txt"
    scratch = workdir / "scratch"
    child_source = (
        "import os,pathlib,subprocess,sys,time;"
        "p=subprocess.Popen([sys.executable,'-c','import time;time.sleep(120)']);"
        f"pathlib.Path({str(pid_file)!r}).write_text(str(p.pid));"
        f"pathlib.Path({str(group_file)!r}).write_text(str(os.getpgrp()));"
        "time.sleep(120)"
    )
    source = f"""
import os
from pathlib import Path
import sys
sys.path.insert(0, {str(Path(native_process.__file__).parent)!r})
from native_process import run_process
scratch = Path({str(scratch)!r})
scratch.mkdir()
try:
    run_process(
        [sys.executable, "-c", {child_source!r}],
        timeout=0.5,
        new_process_group=False,
    )
finally:
    scratch.rmdir()
"""
    try:
        result = native_process.run_process([sys.executable, "-c", source], timeout=3)
        assert result.returncode != 0
        assert "TimeoutExpired" in result.stderr
        assert not scratch.exists()
        assert_process_stopped(int(pid_file.read_text()))
        assert int(group_file.read_text()) != os.getpgrp()
    finally:
        if pid_file.is_file():
            try:
                os.kill(int(pid_file.read_text()), signal.SIGKILL)
            except ProcessLookupError:
                pass


def test_runtime_outer_budget_exceeds_all_inner_limits():
    assert native_process.RUNTIME_TIMEOUT > 7 * native_process.COMMAND_TIMEOUT


def test_standalone_harness_timeout_kills_compiler_descendants(monkeypatch, workdir, harness):
    pid_file = workdir / "standalone-descendant.pid"
    source = (
        "import pathlib,subprocess,sys,time;"
        "p=subprocess.Popen([sys.executable,'-c','import time;time.sleep(120)']);"
        f"pathlib.Path({str(pid_file)!r}).write_text(str(p.pid));"
        "time.sleep(120)"
    )
    monkeypatch.setattr(harness, "TIMEOUT", 0.5)
    try:
        with pytest.raises(RuntimeError, match="timed out after 0.5s"):
            harness.run_command([sys.executable, "-c", source], os.environ.copy())
        assert pid_file.is_file()
        assert_process_stopped(int(pid_file.read_text()))
    finally:
        if pid_file.is_file():
            try:
                os.kill(int(pid_file.read_text()), signal.SIGKILL)
            except ProcessLookupError:
                pass


def test_runtime_lane_uses_group_wrapper_and_checks_artifact_before_execution(monkeypatch, workdir):
    script = Path(__file__).parents[1] / "test_runtime.py"
    spec = importlib.util.spec_from_file_location("runtime_lane", script)
    runtime = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(runtime)
    calls = []
    options = {"--libc": "musl", "--readelf": "/tools/readelf", "--cc": "cc"}
    class Config:
        def getoption(self, name):
            return options[name]
    def validate(library, libc, readelf):
        calls.append(("validate", library, libc, readelf))
    def execute(command, timeout):
        assert calls and calls[0][0] == "validate"
        assert "--inherited-process-group" in command
        assert timeout == native_process.RUNTIME_TIMEOUT
        return subprocess.CompletedProcess(
            command, 0,
            "PASS: negative control was interposed (exit 86)\n"
            + "PASS: bundled font creation and OpenType functions\n" * 3, "",
        )
    monkeypatch.setattr(runtime, "assert_runtime_host", validate)
    monkeypatch.setattr(runtime, "run_tool", execute)
    runtime.test_harfbuzz_symbol_binding_runtime(workdir / "artifact.so", Config())
    def mismatch(*args):
        raise AssertionError("Artifact libc glibc does not match requested libc musl")
    monkeypatch.setattr(runtime, "assert_runtime_host", mismatch)
    monkeypatch.setattr(runtime, "run_tool", lambda *args, **kwargs: pytest.fail("Native execution preceded libc validation"))
    with pytest.raises(AssertionError, match="Artifact libc glibc"):
        runtime.test_harfbuzz_symbol_binding_runtime(workdir / "artifact.so", Config())


def test_outer_timeout_preserves_harness_scratch_cleanup(workdir, harness):
    pid_file = workdir / "compiler-descendant.pid"
    compiler = workdir / "sleeping-compiler.py"
    compiler.write_text(
        "import pathlib,subprocess,sys,time\n"
        "child=subprocess.Popen([sys.executable,'-c','import time;time.sleep(120)'])\n"
        f"pathlib.Path({str(pid_file)!r}).write_text(str(child.pid))\n"
        "time.sleep(120)\n",
        encoding="utf-8",
    )
    library = workdir / "library.so"
    library.write_bytes(b"fixture")
    env = os.environ.copy()
    env["TMPDIR"] = str(workdir)
    with pytest.raises(subprocess.TimeoutExpired):
        native_process.run_process(
            [sys.executable, harness.__file__, library, "--cc",
             f'"{sys.executable}" "{compiler}"', "--inherited-process-group"],
            env=env, timeout=0.75,
        )
    assert pid_file.exists()
    assert_process_stopped(int(pid_file.read_text()))
    assert not list(workdir.glob(".harfbuzz-symbol-binding-*"))
