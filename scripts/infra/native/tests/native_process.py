"""Bound subprocesses and clean up their POSIX process groups."""

import os
import signal
import subprocess
import time


COMMAND_TIMEOUT = 60
# Three compilations and four probes, plus time for cleanup and interpreter startup.
RUNTIME_TIMEOUT = 8 * COMMAND_TIMEOUT


def _signal_group(pid, sig):
    try:
        os.killpg(pid, sig)
    except ProcessLookupError:
        pass


def _stop(process, owns_group):
    if owns_group:
        _signal_group(process.pid, signal.SIGTERM)
    elif process.poll() is None:
        process.kill()
    try:
        process.wait(timeout=5 if owns_group else 1)
    except subprocess.TimeoutExpired:
        process.kill()
        process.wait(timeout=1)
    finally:
        if owns_group:
            _signal_group(process.pid, signal.SIGKILL)


def _text(output):
    return output.decode("utf-8", errors="replace") if isinstance(output, bytes) else output or ""


def run_process(command, env=None, timeout=COMMAND_TIMEOUT, new_process_group=True):
    command = [str(argument) for argument in command]
    owns_group = os.name == "posix" and new_process_group
    process = subprocess.Popen(
        command, env=env, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        start_new_session=owns_group,
    )
    deadline = time.monotonic() + timeout
    stdout, stderr = b"", b""
    try:
        while True:
            remaining = max(0, deadline - time.monotonic())
            try:
                stdout, stderr = process.communicate(timeout=min(0.2, remaining))
                return subprocess.CompletedProcess(
                    command, process.returncode, _text(stdout), _text(stderr)
                )
            except subprocess.TimeoutExpired as error:
                stdout, stderr = error.output or b"", error.stderr or b""
                # A descendant may keep a pipe open after the command exits.
                # The owning wrapper cleans the whole group even on normal exit.
                if process.poll() is not None:
                    return subprocess.CompletedProcess(
                        command, process.returncode, _text(stdout), _text(stderr)
                    )
                if time.monotonic() >= deadline:
                    raise subprocess.TimeoutExpired(
                        command, timeout, output=_text(stdout), stderr=_text(stderr)
                    ) from error
    finally:
        _stop(process, owns_group)
        process.stdout.close()
        process.stderr.close()
