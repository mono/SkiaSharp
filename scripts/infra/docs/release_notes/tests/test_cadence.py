from __future__ import annotations

import importlib.util
import io
import json
import unittest
from pathlib import Path
from unittest import mock


_DOCS_DIR = Path(__file__).resolve().parents[2]


def _load_script(filename, module_name):
    path = _DOCS_DIR / filename
    spec = importlib.util.spec_from_file_location(module_name, str(path))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class CadenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.index = _load_script("release-notes-index.py", "_rn_index_cadence")
        cls.renderer = _load_script("release-notes-render.py", "_rn_render_cadence")

    def test_chrome_schedule_maps_to_release_dates(self):
        payload = {
            "mstones": [{
                "branch_point": "2026-08-17T00:00:00",
                "earliest_beta": "2026-08-19T00:00:00",
                "early_stable_cut": "2026-08-25T00:00:00",
                "stable_cut": "2026-09-01T00:00:00",
                "stable_date": "2026-09-08T00:00:00",
            }]
        }
        response = io.BytesIO(json.dumps(payload).encode("utf-8"))
        response.status = 200
        with mock.patch.object(
                self.index.urllib.request,
                "urlopen",
                return_value=response):
            schedule = self.index.fetch_chrome_schedule(153)

        self.assertEqual({
            "earliest_beta": "2026-08-19T00:00:00",
            "early_stable_cut": "2026-08-25T00:00:00",
            "stable_cut": "2026-09-01T00:00:00",
            "stable_date": "2026-09-08T00:00:00",
        }, schedule)

    def test_timeline_renders_four_release_stages(self):
        schedule = {
            "153": {
                "earliest_beta": "2026-08-19T00:00:00",
                "early_stable_cut": "2026-08-25T00:00:00",
                "stable_cut": "2026-09-01T00:00:00",
                "stable_date": "2026-09-08T00:00:00",
            },
            "154": {
                "earliest_beta": "2026-09-02T00:00:00",
                "early_stable_cut": "2026-09-08T00:00:00",
                "stable_cut": "2026-09-15T00:00:00",
                "stable_date": "2026-09-22T00:00:00",
            },
        }
        rendered = self.renderer.render_cadence_timeline(
            153, 154, "4.153", "4.154", schedule)

        rows = [line for line in rendered if line.startswith("| ")][1:]
        self.assertEqual([
            "| Aug 19 | m153 Earliest Beta | Preview 1 | `4.153.0-preview.1` |",
            "| Aug 25 | m153 Early Stable Cut | Preview 2 | `4.153.0-preview.2` |",
            "| Sep 1 | m153 Stable Cut | RC 1 | `4.153.0-rc.1` |",
            "| Sep 2 | m154 Earliest Beta | Preview 1 | `4.154.0-preview.1` |",
            "| Sep 8 | m153 Stable Date | Stable | `4.153.0` |",
            "| Sep 8 | m154 Early Stable Cut | Preview 2 | `4.154.0-preview.2` |",
            "| Sep 15 | m154 Stable Cut | RC 1 | `4.154.0-rc.1` |",
            "| Sep 22 | m154 Stable Date | Stable | `4.154.0` |",
        ], rows)

    def test_timeline_rejects_stale_schedule_keys(self):
        stale = {
            "153": {"beta": "2026-08-19"},
            "154": {"beta": "2026-09-02"},
        }
        with self.assertRaisesRegex(
                RuntimeError,
                "missing Chromium marker 'earliest_beta' for m153.*release-notes-index.py"):
            self.renderer.render_cadence_timeline(
                153, 154, "4.153", "4.154", stale)
