from __future__ import annotations

import importlib.util
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


class _Response:
    status = 200

    def __init__(self, payload):
        self.payload = payload

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        return False

    def read(self):
        return json.dumps(self.payload).encode("utf-8")


class CadenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.index = _load_script("release-notes-index.py", "_rn_index_cadence")
        cls.renderer = _load_script("release-notes-render.py", "_rn_render_cadence")

    def test_chrome_schedule_maps_to_release_dates(self):
        payload = {
            "mstones": [{
                "branch_point": "2026-08-17T00:00:00",
                "early_stable_cut": "2026-08-25T00:00:00",
                "stable_cut": "2026-09-01T00:00:00",
                "stable_date": "2026-09-08T00:00:00",
            }]
        }
        with mock.patch.object(
                self.index.urllib.request,
                "urlopen",
                return_value=_Response(payload)):
            schedule = self.index.fetch_chrome_schedule(153)

        self.assertEqual({
            "preview_1": "2026-08-18",
            "preview_2": "2026-08-25",
            "rc": "2026-09-01",
            "stable": "2026-09-08",
        }, schedule)

    def test_timeline_renders_four_release_stages(self):
        schedule = {
            "153": {
                "preview_1": "2026-08-18",
                "preview_2": "2026-08-25",
                "rc": "2026-09-01",
                "stable": "2026-09-08",
            },
            "154": {
                "preview_1": "2026-09-01",
                "preview_2": "2026-09-08",
                "rc": "2026-09-15",
                "stable": "2026-09-22",
            },
        }
        rendered = self.renderer.render_cadence_timeline(
            153, 154, "4.153", "4.154", schedule)

        rows = [line for line in rendered if line.startswith("| ")][1:]
        self.assertEqual(8, len(rows))
        self.assertIn("m153 Preview 1", "\n".join(rows))
        self.assertIn("m153 Preview 2", "\n".join(rows))
        self.assertIn("m153 RC 1", "\n".join(rows))
        self.assertIn("m153 Stable", "\n".join(rows))


if __name__ == "__main__":
    unittest.main()
