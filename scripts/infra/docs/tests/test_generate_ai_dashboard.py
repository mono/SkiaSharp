#!/usr/bin/env python3

import copy
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path
from unittest import mock


REPO_ROOT = Path(__file__).resolve().parents[4]
SCRIPT_PATH = REPO_ROOT / "scripts" / "infra" / "docs" / "generate-ai-dashboard.py"


def load_dashboard_module():
    spec = importlib.util.spec_from_file_location("generate_ai_dashboard", SCRIPT_PATH)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


DASHBOARD = load_dashboard_module()


def identity_for(repository):
    return DASHBOARD.resolve_identity(REPO_ROOT, repository=repository)


class DashboardRenderingTests(unittest.TestCase):
    def setUp(self):
        self.existing = {
            "generatedAt": "2026-08-01",
            "adoption": {
                "asOf": "2026-07-31",
                "series": [{"version": "4.100", "downloads": 123}],
            },
            "cadence": {
                "asOf": "2026-07-30",
                "prsUrl": "https://github.com/mono/SkiaSharp/pulls?legacy=true",
                "caption": "Historical cadence caption",
                "milestones": [
                    {
                        "milestone": 150,
                        "prNumber": 4900,
                        "prOpened": "2026-07-01",
                        "prMerged": "2026-07-02",
                        "note": "Historical milestone note",
                    }
                ],
            },
            "cost": {
                "asOf": "2026-07-29",
                "runs": [{"workflow": "Skia Update", "tokens": 456}],
            },
            "footerUrl": "https://github.com/mono/SkiaSharp/tree/legacy/workflows",
        }

    def render(self, repository):
        with (
            mock.patch.object(
                DASHBOARD,
                "build_adoption",
                return_value=copy.deepcopy(self.existing["adoption"]),
            ),
            mock.patch.object(
                DASHBOARD,
                "build_cadence",
                return_value=copy.deepcopy(self.existing["cadence"]),
            ),
            mock.patch.object(
                DASHBOARD,
                "build_cost",
                return_value=copy.deepcopy(self.existing["cost"]),
            ),
            mock.patch.object(DASHBOARD, "today_iso", return_value="2026-09-05"),
        ):
            return DASHBOARD.build_dashboard(
                copy.deepcopy(self.existing),
                identity_for(repository),
            )

    def test_active_links_follow_current_repository(self):
        for repository in ("mono/SkiaSharp", "dotnet/SkiaSharp"):
            with self.subTest(repository=repository):
                rendered = self.render(repository)
                repository_url = f"https://github.com/{repository}"
                self.assertEqual(
                    f"{repository_url}/pulls?q=is%3Apr+milestone+in%3Atitle",
                    rendered["cadence"]["prsUrl"],
                )
                self.assertEqual(
                    f"{repository_url}/tree/main/.github/workflows",
                    rendered["footerUrl"],
                )

    def test_historical_dashboard_data_is_preserved(self):
        rendered = self.render("dotnet/SkiaSharp")
        self.assertEqual(self.existing["adoption"], rendered["adoption"])
        self.assertEqual(self.existing["cost"], rendered["cost"])

        cadence = copy.deepcopy(rendered["cadence"])
        cadence["prsUrl"] = self.existing["cadence"]["prsUrl"]
        self.assertEqual(self.existing["cadence"], cadence)

    def test_main_writes_portable_links_without_rewriting_history(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            base = root / "base.json"
            output = root / "output.json"
            base.write_text(json.dumps(self.existing), encoding="utf-8")

            with (
                mock.patch.object(
                    DASHBOARD,
                    "build_adoption",
                    side_effect=lambda existing: copy.deepcopy(existing["adoption"]),
                ),
                mock.patch.object(
                    DASHBOARD,
                    "build_cadence",
                    side_effect=lambda existing: copy.deepcopy(existing["cadence"]),
                ),
                mock.patch.object(
                    DASHBOARD,
                    "build_cost",
                    side_effect=lambda existing: copy.deepcopy(existing["cost"]),
                ),
                mock.patch.object(DASHBOARD, "today_iso", return_value="2026-09-05"),
            ):
                result = DASHBOARD.main(
                    ["--base", str(base), "--output", str(output)],
                    identity=identity_for("dotnet/SkiaSharp"),
                )

            self.assertEqual(0, result)
            rendered = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(self.existing["adoption"], rendered["adoption"])
            self.assertEqual(self.existing["cost"], rendered["cost"])
            self.assertEqual(
                "Historical milestone note",
                rendered["cadence"]["milestones"][0]["note"],
            )
            self.assertEqual(
                "https://github.com/dotnet/SkiaSharp/tree/main/.github/workflows",
                rendered["footerUrl"],
            )


if __name__ == "__main__":
    unittest.main()
