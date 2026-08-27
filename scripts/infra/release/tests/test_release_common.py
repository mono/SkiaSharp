from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_common as common


class DigestTests(unittest.TestCase):
    def test_with_digest_is_deterministic(self):
        plan = {"b": 2, "a": 1}
        stamped = common.with_digest(plan)
        self.assertEqual(stamped["a"], 1)
        self.assertEqual(stamped["b"], 2)
        self.assertRegex(stamped["planDigest"], r"^[0-9a-f]{64}$")
        # Key order must not affect the digest.
        reordered = common.with_digest({"a": 1, "b": 2})
        self.assertEqual(stamped["planDigest"], reordered["planDigest"])

    def test_verify_digest_accepts_untampered_plan(self):
        plan = common.with_digest({"value": 1})
        common.verify_digest(plan)  # must not raise

    def test_verify_digest_rejects_tampered_field(self):
        plan = common.with_digest({"value": 1})
        plan["value"] = 2
        with self.assertRaisesRegex(common.ValidationError, "digest mismatch"):
            common.verify_digest(plan)

    def test_verify_digest_rejects_missing_digest(self):
        with self.assertRaisesRegex(common.ValidationError, "missing"):
            common.verify_digest({"value": 1})

    def test_verify_digest_rejects_tampered_new_field(self):
        plan = common.with_digest({"value": 1})
        plan["extra"] = "injected"
        with self.assertRaisesRegex(common.ValidationError, "digest mismatch"):
            common.verify_digest(plan)


class SchemaRoundTripTests(unittest.TestCase):
    def test_write_then_read_plan_round_trips(self):
        import tempfile

        schema_dir = common.SCHEMA_DIR
        schema_name = "roundtrip-test.schema.json"
        schema_path = schema_dir / schema_name
        schema_path.write_text(
            '{"type": "object", "required": ["value"], '
            '"properties": {"value": {"type": "integer"}}}',
            encoding="utf-8",
        )
        try:
            with tempfile.TemporaryDirectory() as tmp:
                plan_path = Path(tmp) / "plan.json"
                written = common.write_plan(plan_path, {"value": 42}, schema_name=schema_name)
                self.assertEqual(written["value"], 42)
                read_back = common.read_plan(plan_path, schema_name=schema_name)
                self.assertEqual(read_back, written)
        finally:
            schema_path.unlink()

    def test_write_plan_rejects_schema_violation(self):
        schema_dir = common.SCHEMA_DIR
        schema_name = "roundtrip-invalid-test.schema.json"
        schema_path = schema_dir / schema_name
        schema_path.write_text(
            '{"type": "object", "required": ["value"], '
            '"properties": {"value": {"type": "integer"}}}',
            encoding="utf-8",
        )
        try:
            with self.assertRaises(common.ValidationError):
                common.write_plan(Path("/dev/null/unused.json"), {"value": "not-an-int"}, schema_name=schema_name)
        finally:
            schema_path.unlink()

    def test_read_plan_rejects_hand_edited_file(self):
        import tempfile

        schema_dir = common.SCHEMA_DIR
        schema_name = "roundtrip-tamper-test.schema.json"
        schema_path = schema_dir / schema_name
        schema_path.write_text(
            '{"type": "object", "required": ["value"], '
            '"properties": {"value": {"type": "integer"}}}',
            encoding="utf-8",
        )
        try:
            with tempfile.TemporaryDirectory() as tmp:
                plan_path = Path(tmp) / "plan.json"
                common.write_plan(plan_path, {"value": 42}, schema_name=schema_name)
                text = plan_path.read_text(encoding="utf-8").replace("42", "99")
                plan_path.write_text(text, encoding="utf-8")
                with self.assertRaisesRegex(common.ValidationError, "digest mismatch"):
                    common.read_plan(plan_path, schema_name=schema_name)
        finally:
            schema_path.unlink()


class CommandRunnerTests(unittest.TestCase):
    def test_subprocess_runner_raises_on_failure(self):
        runner = common.SubprocessCommandRunner()
        with self.assertRaises(common.ReleaseToolError):
            runner.run(["false"], cwd=Path.cwd())

    def test_subprocess_runner_returns_output_on_success(self):
        runner = common.SubprocessCommandRunner()
        result = runner.run(["echo", "hello"], cwd=Path.cwd())
        self.assertEqual(result.stdout.strip(), "hello")
        self.assertTrue(result.ok)


class BuildEnvelopeTests(unittest.TestCase):
    def _plan(self) -> dict:
        return common.with_digest(
            {
                "toolingSha": "a" * 40,
                "release": {
                    "identity": "3.119.0-preview.1",
                    "version": "3.119.0-preview.1.42",
                    "branch": "release/3.119.0-preview.1",
                },
            }
        )

    def test_build_envelope_carries_standardized_fields(self):
        plan = self._plan()
        envelope = common.build_envelope(plan, next_action="publish", tag="v3.119.0-preview.1")
        self.assertEqual(envelope["toolingSha"], "a" * 40)
        self.assertEqual(envelope["planDigest"], plan["planDigest"])
        self.assertEqual(envelope["nextAction"], "publish")
        self.assertEqual(envelope["release"]["identity"], "3.119.0-preview.1")
        self.assertEqual(envelope["release"]["version"], "3.119.0-preview.1.42")
        self.assertEqual(envelope["release"]["branch"], "release/3.119.0-preview.1")
        self.assertEqual(envelope["tag"], "v3.119.0-preview.1")

    def test_build_envelope_validates_its_own_output(self):
        plan = self._plan()
        # An invalid toolingSha would violate result-envelope.schema.json;
        # build_envelope must catch that defensively even though it produced
        # the envelope itself.
        broken_plan = dict(plan)
        broken_plan["toolingSha"] = "not-a-sha"
        with self.assertRaises(common.ValidationError):
            common.build_envelope(broken_plan, next_action="done")

    def test_validate_result_envelope_accepts_well_formed_document(self):
        envelope = common.build_envelope(self._plan(), next_action="done")
        common.validate_result_envelope(envelope)  # must not raise

    def test_validate_result_envelope_rejects_missing_next_action(self):
        envelope = common.build_envelope(self._plan(), next_action="done")
        del envelope["nextAction"]
        with self.assertRaises(common.ValidationError):
            common.validate_result_envelope(envelope)


if __name__ == "__main__":
    unittest.main()
