import unittest

from run_review import (
    extract_skia_milestone_from_cgmanifest,
    extract_skia_upstream_commit_from_cgmanifest,
)


class RunReviewTests(unittest.TestCase):
    def test_extracts_exact_skia_registration(self) -> None:
        manifest = {
            "registrations": [
                {"component": {"other": {"name": "other", "version": "1"}}},
                {
                    "component": {
                        "other": {
                            "name": "skia",
                            "version": "chrome/m152",
                        }
                    },
                    "chrome_milestone": 152,
                    "upstream_merge_commit": "abc123",
                },
            ]
        }

        self.assertEqual(
            "chrome/m152", extract_skia_milestone_from_cgmanifest(manifest)
        )
        self.assertEqual(
            "abc123", extract_skia_upstream_commit_from_cgmanifest(manifest)
        )

    def test_returns_none_when_skia_registration_is_missing(self) -> None:
        manifest = {"registrations": []}

        self.assertIsNone(extract_skia_milestone_from_cgmanifest(manifest))
        self.assertIsNone(extract_skia_upstream_commit_from_cgmanifest(manifest))


if __name__ == "__main__":
    unittest.main()
