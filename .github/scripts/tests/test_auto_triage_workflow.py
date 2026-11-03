"""Check the unattended triage tool and output contract."""

from pathlib import Path
import unittest

import yaml


ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / ".github/workflows/auto-triage.md"


class AutoTriageWorkflowTests(unittest.TestCase):
    def test_required_report_commands_are_available_without_interactive_approval(self):
        source = SOURCE.read_text(encoding="utf-8")
        workflow = yaml.safe_load(source.split("---", 2)[1])
        tools = workflow["tools"]
        self.assertIn("edit", tools)
        self.assertTrue({"python3", "mkdir", "cp", "cat", "rg"} <= set(tools["bash"]))
        self.assertNotIn("*", tools["bash"])
        lock = SOURCE.with_suffix(".lock.yml").read_text(encoding="utf-8")
        for command in ("python3", "mkdir", "cp", "cat", "rg"):
            self.assertIn(f"# --allow-tool shell({command})", lock)

    def test_schema_dependencies_are_provisioned_before_agent_and_output_path_matches_skill(self):
        source = SOURCE.read_text(encoding="utf-8")
        workflow = yaml.safe_load(source.split("---", 2)[1])
        self.assertIn("requirements.txt", str(workflow["pre-agent-steps"]))
        requirements = ROOT / ".agents/skills/issue-triage/scripts/requirements.txt"
        self.assertIn("jsonschema", requirements.read_text(encoding="utf-8"))
        self.assertIn("output/ai-triage/${{", source)
        self.assertNotIn("issue-triage-workspace/", source)
        self.assertEqual(2, source.count("-label:agentic-workflows"))


if __name__ == "__main__":
    unittest.main()
