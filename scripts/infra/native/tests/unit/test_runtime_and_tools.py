from pathlib import Path
import platform
import shlex
import struct
import subprocess
import sys

import pytest

import artifact_checks as checks


def elf_header(machine=62, elf_class=2, byte_order=1):
    header = bytearray(20)
    header[:7] = bytes([0x7f, 0x45, 0x4c, 0x46, elf_class, byte_order, 1])
    header[18:20] = struct.pack("<H" if byte_order == 1 else ">H", machine)
    return header


@pytest.fixture
def library(workdir):
    path = workdir / "libHarfBuzzSharp.so"
    path.write_bytes(elf_header())
    return path


@pytest.mark.parametrize("name", ["not-installed-native-tool-5e1f", "/missing/native/tool"])
def test_missing_tools_fail(name):
    with pytest.raises(AssertionError, match="Required tool not found"):
        checks.require_tool(name)


def test_missing_empty_and_relative_artifacts_fail(workdir):
    with pytest.raises(AssertionError, match="does not exist"):
        checks.require_file(workdir / "missing.so")
    path = workdir / "empty.so"
    path.touch()
    with pytest.raises(AssertionError, match="empty"):
        checks.require_file(path)
    with pytest.raises(AssertionError, match="absolute"):
        checks.require_file("relative.so")
    with pytest.raises(AssertionError, match="not a file"):
        checks.require_file(workdir)


def test_tool_commands_are_bounded_and_locale_independent(monkeypatch):
    def run(command, **kwargs):
        assert command == ["tool", "arg with spaces"]
        assert kwargs["timeout"] == 60
        assert kwargs["env"]["LC_ALL"] == "C"
        assert "shell" not in kwargs
        return subprocess.CompletedProcess(command, 0, "output", "")
    monkeypatch.setattr(checks, "run_process", run)
    assert checks.run_tool(["tool", "arg with spaces"]).stdout == "output"


@pytest.mark.parametrize("exception,diagnostic", [
    (subprocess.TimeoutExpired("probe", 60, output="partial", stderr="stuck"), "timed out after 60s"),
    (FileNotFoundError("missing executable"), "Could not execute"),
])
def test_tool_failures_have_diagnostics(monkeypatch, exception, diagnostic):
    def run(*args, **kwargs):
        raise exception
    monkeypatch.setattr(checks, "run_process", run)
    with pytest.raises(AssertionError, match=diagnostic):
        checks.run_tool(["probe"])


def test_tool_nonzero_reports_stdout_and_stderr(monkeypatch):
    monkeypatch.setattr(
        checks, "run_process",
        lambda *args, **kwargs: subprocess.CompletedProcess(args[0], 9, "details", "reason"),
    )
    with pytest.raises(AssertionError, match=r"(?s)failed \(9\).*details.*reason"):
        checks.run_tool(["probe"])


def test_gnu_readelf_required(monkeypatch):
    monkeypatch.setattr(checks, "require_tool", lambda command: command)
    monkeypatch.setattr(
        checks, "run_tool",
        lambda command: subprocess.CompletedProcess(command, 0, "LLVM readelf version 19", ""),
    )
    with pytest.raises(AssertionError, match="GNU readelf is required"):
        checks.gnu_readelf("llvm-readelf")


def test_runtime_non_linux_fails_without_execution(monkeypatch, library):
    monkeypatch.setattr(checks.platform, "system", lambda: "Darwin")
    with pytest.raises(AssertionError, match="requires a Linux host"):
        checks.assert_runtime_host(library, "glibc")


@pytest.mark.parametrize("machine,elf_class,byte_order", [(183, 2, 1), (62, 1, 1), (62, 2, 2)])
def test_runtime_architecture_mismatch_fails(monkeypatch, library, machine, elf_class, byte_order):
    library.write_bytes(elf_header(machine, elf_class, byte_order))
    monkeypatch.setattr(checks.platform, "system", lambda: "Linux")
    monkeypatch.setattr(checks.platform, "machine", lambda: "x86_64")
    with pytest.raises(AssertionError, match="does not match host"):
        checks.assert_runtime_host(library, "glibc")


def test_runtime_libc_mismatch_fails(monkeypatch, library):
    monkeypatch.setattr(checks.platform, "system", lambda: "Linux")
    monkeypatch.setattr(checks.platform, "machine", lambda: "x86_64")
    monkeypatch.setattr(checks, "detect_host_libc", lambda: "musl")
    with pytest.raises(AssertionError, match="Expected libc glibc does not match host libc musl"):
        checks.assert_runtime_host(library, "glibc")


@pytest.mark.parametrize("machine,elf_class,byte_order,host", [
    (62, 2, 1, "x86_64"), (183, 2, 1, "aarch64"),
    (3, 1, 1, "i686"), (40, 1, 1, "armv7l"), (22, 2, 2, "s390x"),
])
def test_matching_runtime_host_passes(monkeypatch, library, machine, elf_class, byte_order, host):
    library.write_bytes(elf_header(machine, elf_class, byte_order))
    monkeypatch.setattr(checks.platform, "system", lambda: "Linux")
    monkeypatch.setattr(checks.platform, "machine", lambda: host)
    monkeypatch.setattr(checks, "detect_host_libc", lambda: "glibc")
    monkeypatch.setattr(checks, "inspect_artifact_libc", lambda library, readelf: "glibc")
    checks.assert_runtime_host(library, "glibc")


def test_controlled_compiler_built_elf_is_inspectable_without_loading(workdir, pytestconfig):
    compiler = shlex.split(pytestconfig.getoption("--cc"))
    assert compiler, "--cc must name a C compiler"
    compiler[0] = checks.require_tool(compiler[0])
    source = workdir / "fixture.c"
    artifact = workdir / "fixture.o"
    source.write_text("int hb_fixture(void) { return 42; }\n", encoding="utf-8")
    target = ["--target=x86_64-linux-gnu"] if platform.system() == "Darwin" else []
    checks.run_tool(compiler + target + ["-fPIC", "-c", source, "-o", artifact])
    machine, elf_class, byte_order = checks.read_elf_machine(artifact)
    assert machine > 0 and elf_class in (1, 2) and byte_order in (1, 2)
    if target:
        assert (machine, elf_class, byte_order) == (62, 2, 1)


@pytest.mark.parametrize("contents", [b"MZ not ELF", b"\x7fELF", bytes(20)])
def test_malformed_elf_fails(library, contents):
    library.write_bytes(contents)
    with pytest.raises(AssertionError, match="not ELF"):
        checks.read_elf_machine(library)


@pytest.mark.parametrize("description,returncode,expected", [
    ("ldd (Debian GLIBC 2.36) 2.36", 0, "glibc"),
    ("musl libc (x86_64)\nVersion 1.2.5", 1, "musl"),
])
def test_detect_libc_via_ldd(monkeypatch, description, returncode, expected):
    monkeypatch.setattr(checks.platform, "libc_ver", lambda: ("", ""))
    monkeypatch.setattr(checks, "require_tool", lambda command: command)
    def run(command, **kwargs):
        assert command == ["ldd", "--version"] and kwargs["check"] is False
        return subprocess.CompletedProcess(command, returncode, "", description)
    monkeypatch.setattr(checks, "run_tool", run)
    assert checks.detect_host_libc() == expected


def test_unknown_libc_fails(monkeypatch):
    monkeypatch.setattr(checks.platform, "libc_ver", lambda: ("", ""))
    monkeypatch.setattr(checks, "require_tool", lambda command: command)
    monkeypatch.setattr(
        checks, "run_tool",
        lambda *args, **kwargs: subprocess.CompletedProcess(args[0], 1, "", "unknown ldd"),
    )
    with pytest.raises(AssertionError, match="Could not identify host libc"):
        checks.detect_host_libc()


def test_negative_control_must_exit_86_and_identify_interposition(monkeypatch, harness):
    calls = []
    def run(command, env, expected=0):
        calls.append((command, expected))
        return subprocess.CompletedProcess(command, expected, "", harness.INTERPOSITION_MESSAGE)
    monkeypatch.setattr(harness, "run_command", run)
    harness.verify_negative_control("probe", "fake.so", "interposer.so", {})
    assert calls == [
        (["probe", "fake.so"], 0),
        (["probe", "fake.so", "interposer.so"], 86),
    ]


def test_exit_86_without_interposer_diagnostic_is_not_a_control(monkeypatch, harness):
    monkeypatch.setattr(
        harness, "run_command",
        lambda command, env, expected=0: subprocess.CompletedProcess(command, expected, "", ""),
    )
    with pytest.raises(RuntimeError, match="without the interposer diagnostic"):
        harness.verify_negative_control("probe", "fake.so", "interposer.so", {})


def test_negative_control_wrong_exit_fails(monkeypatch, harness):
    monkeypatch.setattr(
        harness, "run_process",
        lambda command, **kwargs: subprocess.CompletedProcess(command, 0, "", ""),
    )
    with pytest.raises(RuntimeError, match="returned 0, expected 86"):
        harness.verify_negative_control("probe", "fake.so", "interposer.so", {})


def test_original_harness_timeouts_apply_to_compile_and_probe(monkeypatch, harness):
    calls = []
    def run(command, **kwargs):
        calls.append(command)
        assert kwargs["timeout"] == 60
        assert "shell" not in kwargs
        raise subprocess.TimeoutExpired(command, 60, stderr="stalled")
    monkeypatch.setattr(harness, "run_process", run)
    for command in [["cc", "probe.c"], ["probe", "library.so"]]:
        with pytest.raises(RuntimeError, match="timed out after 60s"):
            harness.run_command(command, {})
    assert len(calls) == 2


def test_harness_checks_controls_before_actual_library(monkeypatch, harness, library):
    monkeypatch.setattr(sys, "argv", ["test-harfbuzz-symbol-binding.py", str(library), "--cc", 'ccache cc "-DNAME=a b"'])
    monkeypatch.setattr(harness.tempfile, "tempdir", str(library.parent))
    monkeypatch.setenv("LD_PRELOAD", "unwanted.so")
    monkeypatch.setenv("LD_AUDIT", "unwanted-audit.so")
    commands = []
    def run(command, env, expected=0):
        commands.append((command, expected))
        assert env["LC_ALL"] == "C"
        assert "LD_PRELOAD" not in env and "LD_AUDIT" not in env
        return subprocess.CompletedProcess(command, expected, "", harness.INTERPOSITION_MESSAGE)
    monkeypatch.setattr(harness, "run_command", run)
    harness.main()
    assert len(commands) == 7
    for command, _ in commands[:3]:
        assert command[:3] == ["ccache", "cc", "-DNAME=a b"]
    assert "-DNEGATIVE_CONTROL" in commands[2][0]
    assert commands[3][0][1].endswith("negative-control.so")
    assert commands[4][1] == 86
    assert commands[5][0][1] == str(library)
    assert commands[6][0][1] == str(library)
    assert all(not Path(command[-1]).exists() for command, _ in commands[:3])


def test_harness_uses_default_temp_directory_and_honors_tmpdir(monkeypatch, harness, library):
    scratch = library.parent / "writable-executable-scratch"
    scratch.mkdir()
    monkeypatch.setenv("TMPDIR", str(scratch))
    monkeypatch.setattr(harness.tempfile, "tempdir", None)
    monkeypatch.setattr(sys, "argv", ["test-harfbuzz-symbol-binding.py", str(library)])
    temporary_directory = harness.tempfile.TemporaryDirectory
    created = []
    def default_directory(**kwargs):
        assert "dir" not in kwargs, "The harness must not require a writable checkout"
        directory = temporary_directory(**kwargs)
        created.append(Path(directory.name))
        assert Path(directory.name).parent == scratch
        return directory
    monkeypatch.setattr(harness.tempfile, "TemporaryDirectory", default_directory)
    monkeypatch.setattr(
        harness, "run_command",
        lambda command, env, expected=0: subprocess.CompletedProcess(
            command, expected, "", harness.INTERPOSITION_MESSAGE
        ),
    )
    harness.main()
    assert len(created) == 1
    assert not created[0].exists()
