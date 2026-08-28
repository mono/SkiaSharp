"""Prove that scripts/infra/release has no third-party Python dependency.

This package must run on a stock GitHub-hosted runner: no ``pip install``
step, no network access, only the Python standard library plus this
repository's own files. ``release_common.py`` used to import the
third-party ``jsonschema`` package for JSON Schema validation; it now uses
the in-repo, standard-library-only :mod:`release_schema` instead (see that
module's docstring for exactly which JSON Schema subset it implements).

A plain unit test that simply imports ``release_common`` in-process is not
a strong enough proof: this test process may have ``jsonschema`` installed
(as it does in this repository's own dev/CI environment) and an in-process
import would silently succeed even if ``release_common`` still imported it.
To actually prove the dependency is gone, every test below spawns a real
subprocess with a ``sys.meta_path`` finder that makes importing
``jsonschema`` (or any of its submodules) raise ``ImportError`` -- as if the
package were not installed at all -- and then exercises real, user-facing
behaviour (schema validation, and the ``release.py`` CLI entry point
itself) inside that subprocess.
"""

from __future__ import annotations

import subprocess
import sys
import unittest
from pathlib import Path

RELEASE_DIR = Path(__file__).resolve().parent.parent

# Installed as sys.meta_path[0] inside the child interpreter, before any of
# this repository's modules are imported. Any attempt to import
# "jsonschema" (or "jsonschema.<anything>") raises ImportError immediately,
# exactly as it would on a runner where the package was never installed.
_BLOCK_JSONSCHEMA = """
import sys


class _BlockJsonschema:
    def find_spec(self, fullname, path, target=None):
        if fullname == "jsonschema" or fullname.startswith("jsonschema."):
            raise ImportError(
                f"blocked import of {fullname!r}: scripts/infra/release must "
                "not require the third-party 'jsonschema' package"
            )
        return None


sys.meta_path.insert(0, _BlockJsonschema())
"""


def _run_isolated(body: str) -> subprocess.CompletedProcess[str]:
    """Run ``body`` in a fresh interpreter with jsonschema import-blocked.

    ``cwd`` is the release package directory itself, matching how every
    other test in this suite imports sibling modules (bare
    ``import release_common`` relying on the current directory being on
    ``sys.path``), and how a thin workflow would invoke
    ``python3 scripts/infra/release/release.py ...`` with that directory
    as the working directory.
    """

    script = _BLOCK_JSONSCHEMA + "\n" + body
    return subprocess.run(
        [sys.executable, "-c", script],
        cwd=str(RELEASE_DIR),
        capture_output=True,
        text=True,
        timeout=30,
    )


class ReleaseCommonImportTests(unittest.TestCase):
    def test_release_common_imports_without_jsonschema(self):
        result = _run_isolated("import release_common\nprint('OK')")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)

    def test_release_schema_module_has_no_third_party_imports(self):
        # release_schema.py is the in-repo validator itself; it must never
        # reach for jsonschema even indirectly.
        result = _run_isolated("import release_schema\nprint('OK')")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)


class SchemaValidationWithoutJsonschemaTests(unittest.TestCase):
    """Exercise real schema-validation behaviour, not just importability."""

    def test_validates_a_well_formed_result_envelope(self):
        result = _run_isolated(
            """
import release_common as common

envelope = common.with_digest(
    {
        "toolingSha": "a" * 40,
        "release": {
            "identity": "3.119.0-preview.1",
            "version": "3.119.0-preview.1.42",
            "branch": "release/3.119.0-preview.1",
        },
    }
)
envelope["nextAction"] = "publish"
common.validate_result_envelope(envelope)  # must not raise
assert "jsonschema" not in __import__("sys").modules
print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)

    def test_rejects_a_malformed_result_envelope(self):
        result = _run_isolated(
            """
import release_common as common

envelope = common.with_digest(
    {
        "toolingSha": "not-a-valid-sha",
        "release": {
            "identity": "3.119.0-preview.1",
            "version": "3.119.0-preview.1.42",
            "branch": "release/3.119.0-preview.1",
        },
    }
)
envelope["nextAction"] = "publish"
try:
    common.validate_result_envelope(envelope)
    print("ERROR: malformed envelope was accepted")
except common.ValidationError as exc:
    assert "toolingSha" in str(exc), str(exc)
    print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)
        self.assertNotIn("ERROR", result.stdout)

    def test_every_real_schema_file_loads_and_is_supported(self):
        # Every schemas/*.json file must be expressible in release_schema's
        # supported keyword subset -- exercised against a trivial {} (or
        # []) instance so this only proves the *schema* itself is
        # supported, independent of any specific plan/result shape.
        result = _run_isolated(
            """
import release_common as common
import release_schema

for schema_name in [
    "prepare-plan.schema.json",
    "finish-plan.schema.json",
    "plan-summary.schema.json",
    "result-envelope.schema.json",
]:
    schema = common.load_schema(schema_name)
    issues = release_schema.validate({}, schema)
    assert issues, f"{schema_name}: an empty object should fail required-field checks"
print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)


class CliEntryPointWithoutJsonschemaTests(unittest.TestCase):
    """Prove the actual ``release.py`` entry point (not just the library) works."""

    def test_top_level_help_runs_without_jsonschema(self):
        result = _run_isolated(
            """
import runpy
import sys

sys.argv = ["release.py", "--help"]
try:
    runpy.run_path("release.py", run_name="__main__")
except SystemExit as exc:
    assert exc.code in (0, None), exc.code
print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)
        self.assertIn("usage:", result.stdout)

    def test_prepare_plan_help_runs_without_jsonschema(self):
        result = _run_isolated(
            """
import runpy
import sys

sys.argv = ["release.py", "prepare", "plan", "--help"]
try:
    runpy.run_path("release.py", run_name="__main__")
except SystemExit as exc:
    assert exc.code in (0, None), exc.code
print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)
        self.assertIn("usage:", result.stdout)

    def test_render_plan_help_runs_without_jsonschema(self):
        result = _run_isolated(
            """
import runpy
import sys

sys.argv = ["release.py", "render-plan", "--help"]
try:
    runpy.run_path("release.py", run_name="__main__")
except SystemExit as exc:
    assert exc.code in (0, None), exc.code
print("OK")
"""
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("OK", result.stdout)
        self.assertIn("usage:", result.stdout)


if __name__ == "__main__":
    unittest.main()
