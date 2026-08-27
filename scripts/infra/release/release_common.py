"""Shared primitives for the release automation CLI.

This module intentionally has no knowledge of Git, GitHub, or NuGet. It only
provides: error types, a subprocess runner that never uses a shell, canonical
JSON digesting, and JSON-schema validation helpers. Every other module in
``scripts/infra/release`` builds on top of these primitives so that behaviour
stays deterministic and testable without touching the network.
"""

from __future__ import annotations

import datetime as _datetime
import hashlib
import json
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Sequence

try:
    from jsonschema import Draft202012Validator
except ImportError as exc:  # pragma: no cover - exercised only when missing
    raise ImportError(
        "the 'jsonschema' package is required by scripts/infra/release; "
        "install it with 'pip install jsonschema'"
    ) from exc


SCHEMA_DIR = Path(__file__).resolve().parent / "schemas"

# The plan document field that carries the canonical digest. It is always
# excluded from the digest computation itself.
DIGEST_FIELD = "digest"


class ReleaseToolError(RuntimeError):
    """Base class for every error raised by the release automation CLI."""


class PlanError(ReleaseToolError):
    """A plan could not be constructed because inputs or state are invalid."""


class ValidationError(ReleaseToolError):
    """A plan or a plan file failed schema or digest validation."""


class ConflictError(ReleaseToolError):
    """Live state conflicts with a plan in a way that must never be forced.

    Raised for anything the plan explicitly forbids recovering from
    automatically: a moved tag, a mismatched existing release, a diverged
    branch, etc. Callers must stop and report recovery instructions; nothing
    in this package reacts to a ``ConflictError`` by forcing state.
    """


class NotReadyError(ReleaseToolError):
    """An external system has not converged yet (for example NuGet indexing).

    Distinguished from :class:`ConflictError` because the caller should
    report a bounded, rerunnable "pending" result rather than a hard failure.
    """


@dataclass(frozen=True)
class CommandResult:
    args: tuple[str, ...]
    returncode: int
    stdout: str
    stderr: str

    @property
    def ok(self) -> bool:
        return self.returncode == 0


class CommandRunner:
    """Runs argv lists directly -- never a shell string.

    Kept as a small injectable object so tests can substitute a fake runner
    when they need to observe or fail a specific invocation, while the real
    CLI always uses :class:`SubprocessCommandRunner`.
    """

    def run(
        self,
        args: Sequence[str],
        *,
        cwd: Path,
        check: bool = True,
        timeout: int = 120,
        input: str | None = None,
    ) -> CommandResult:
        raise NotImplementedError


class SubprocessCommandRunner(CommandRunner):
    """The real runner used outside of tests."""

    def run(
        self,
        args: Sequence[str],
        *,
        cwd: Path,
        check: bool = True,
        timeout: int = 120,
        input: str | None = None,
    ) -> CommandResult:
        args = tuple(args)
        completed = subprocess.run(
            args,
            cwd=str(cwd),
            capture_output=True,
            text=True,
            timeout=timeout,
            input=input,
        )
        result = CommandResult(
            args=args,
            returncode=completed.returncode,
            stdout=completed.stdout,
            stderr=completed.stderr,
        )
        if check and not result.ok:
            detail = result.stderr.strip() or result.stdout.strip() or "no output"
            raise ReleaseToolError(
                f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
            )
        return result


DEFAULT_RUNNER = SubprocessCommandRunner()


def utcnow_iso() -> str:
    return (
        _datetime.datetime.now(tz=_datetime.timezone.utc)
        .isoformat(timespec="seconds")
        .replace("+00:00", "Z")
    )


def canonical_json(value: Any) -> str:
    """Deterministically serialize ``value`` for hashing purposes."""

    return json.dumps(value, sort_keys=True, separators=(",", ":"), ensure_ascii=True)


def compute_digest(plan: dict) -> str:
    """Compute the canonical SHA-256 digest of a plan, excluding the digest field."""

    without_digest = {key: value for key, value in plan.items() if key != DIGEST_FIELD}
    return hashlib.sha256(canonical_json(without_digest).encode("utf-8")).hexdigest()


def with_digest(plan: dict) -> dict:
    """Return a copy of ``plan`` with its canonical digest attached."""

    stamped = dict(plan)
    stamped[DIGEST_FIELD] = compute_digest(plan)
    return stamped


def verify_digest(plan: dict) -> None:
    """Raise :class:`ValidationError` if ``plan``'s stored digest does not match."""

    stored = plan.get(DIGEST_FIELD)
    if not isinstance(stored, str) or not stored:
        raise ValidationError("plan is missing its canonical digest")
    expected = compute_digest(plan)
    if stored != expected:
        raise ValidationError(
            "plan digest mismatch: the plan file was modified after it was "
            f"generated (expected {expected}, found {stored})"
        )


def load_schema(name: str) -> dict:
    path = SCHEMA_DIR / name
    if not path.is_file():
        raise ValidationError(f"schema not found: {path}")
    with path.open(encoding="utf-8") as handle:
        return json.load(handle)


def validate_against_schema(instance: dict, schema_name: str) -> None:
    schema = load_schema(schema_name)
    validator = Draft202012Validator(schema)
    errors = sorted(validator.iter_errors(instance), key=lambda e: list(e.absolute_path))
    if errors:
        details = "; ".join(
            f"{'.'.join(str(p) for p in error.absolute_path) or '(root)'}: {error.message}"
            for error in errors
        )
        raise ValidationError(f"plan failed schema validation ({schema_name}): {details}")


def write_plan(path: Path, plan: dict, *, schema_name: str) -> dict:
    """Validate ``plan`` against its schema, stamp its digest, and write it."""

    validate_against_schema(plan, schema_name)
    stamped = with_digest(plan)
    # Stamping does not change any field the schema constrains other than the
    # digest itself, but re-validate defensively so a schema that happens to
    # constrain "digest" is still honoured.
    validate_against_schema(stamped, schema_name)
    path.write_text(json.dumps(stamped, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    return stamped


def read_plan(path: Path, *, schema_name: str) -> dict:
    """Load, schema-validate, and digest-verify a plan file.

    This is the only supported way apply/execute commands consume a plan
    file; they never interpret arbitrary fields as commands.
    """

    if not path.is_file():
        raise ValidationError(f"plan file not found: {path}")
    try:
        plan = json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        raise ValidationError(f"plan file is not valid JSON: {exc}") from exc
    if not isinstance(plan, dict):
        raise ValidationError("plan file must contain a JSON object")
    validate_against_schema(plan, schema_name)
    verify_digest(plan)
    return plan


def print_json(value: Any) -> None:
    print(json.dumps(value, indent=2, sort_keys=True))


def write_json_file(path: Path, value: Any) -> None:
    """Write ``value`` as deterministic, human-diffable JSON to ``path``.

    Used for command reports that are not themselves a versioned/digested
    plan artifact (apply/publish/closeout/inspect results, rendered plan
    summaries): callers that need a re-consumable, schema-validated plan
    file use :func:`write_plan` instead.
    """

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def emit(value: Any, *, output: Path | None = None) -> None:
    """Print ``value`` to stdout and, when ``output`` is given, also write it
    to that exact path so thin workflows never have to scrape stdout."""

    print_json(value)
    if output is not None:
        write_json_file(output, value)
