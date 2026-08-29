from __future__ import annotations

import sys
import unittest
from pathlib import Path

_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import github, safety


class ValidateProseTextTests(unittest.TestCase):
    def test_accepts_a_normal_sentence(self):
        self.assertEqual(
            safety.validate_prose_text(
                "SkiaSharp 4.151.0 previews the Skia m151 engine update.",
                field="headline",
            ),
            [],
        )

    def test_rejects_non_string(self):
        errors = safety.validate_prose_text(None, field="headline")
        self.assertTrue(any("must be a string" in error for error in errors))

    def test_rejects_empty_or_whitespace_only(self):
        for value in ("", "   ", "\n\t"):
            with self.subTest(value=repr(value)):
                errors = safety.validate_prose_text(value, field="headline")
                self.assertTrue(any("is empty" in error for error in errors))

    def test_rejects_a_code_fence(self):
        errors = safety.validate_prose_text("```\nsome code\n```", field="body")
        self.assertTrue(any("code fence" in error for error in errors))

    def test_rejects_every_managed_marker(self):
        for marker in (
            github.SUMMARY_START_MARKER,
            github.SUMMARY_END_MARKER,
            github.GENERATED_START_MARKER,
            github.GENERATED_END_MARKER,
            safety.RELEASE_LINKS_MARKER,
        ):
            with self.subTest(marker=marker):
                errors = safety.validate_prose_text(
                    "A normal sentence {} with a marker.".format(marker), field="body"
                )
                self.assertTrue(any("managed marker" in error for error in errors))

    def test_rejects_any_html_comment_not_just_known_markers(self):
        errors = safety.validate_prose_text(
            "A sentence <!-- injected --> continues.", field="body"
        )
        self.assertTrue(any("HTML comment" in error for error in errors))

    def test_rejects_cve_claims(self):
        errors = safety.validate_prose_text(
            "Fixes CVE-2024-12345 in the bundled library.", field="body"
        )
        self.assertTrue(any("security or vulnerability" in error for error in errors))

    def test_rejects_security_fix_claims(self):
        for phrase in (
            "This is a security fix for libpng.",
            "A security release addressing memory corruption.",
            "Patches a known vulnerability in libwebp.",
        ):
            with self.subTest(phrase=phrase):
                errors = safety.validate_prose_text(phrase, field="body")
                self.assertTrue(any("security or vulnerability" in error for error in errors))

    def test_accepts_neutral_dependency_wording(self):
        # The neutral phrasing the retired teaser guide required is still safe.
        errors = safety.validate_prose_text("Updated libpng to 1.6.44.", field="body")
        self.assertEqual(errors, [])

    def test_rejects_unwritten_placeholders(self):
        for placeholder in ("Replace this comment", "TBD", "TODO", "Lorem ipsum dolor", "None."):
            with self.subTest(placeholder=placeholder):
                errors = safety.validate_prose_text(placeholder, field="headline")
                self.assertTrue(any("placeholder" in error for error in errors))

    def test_rejects_a_heading_as_the_opening_line(self):
        errors = safety.validate_prose_text("## What's New\n- a bullet", field="body")
        self.assertTrue(any("plain-language sentence" in error for error in errors))

    def test_rejects_a_list_marker_as_the_opening_line(self):
        errors = safety.validate_prose_text("- a bullet point first", field="body")
        self.assertTrue(any("plain-language sentence" in error for error in errors))

    def test_enforces_a_word_cap(self):
        text = " ".join(["word"] * 10)
        errors = safety.validate_prose_text(text, field="headline", max_words=5)
        self.assertTrue(any("cap 5" in error for error in errors))

    def test_no_word_cap_means_no_length_error(self):
        text = " ".join(["word"] * 1000)
        errors = safety.validate_prose_text(text, field="body")
        self.assertEqual(errors, [])

    def test_reports_every_violation_at_once(self):
        errors = safety.validate_prose_text("```CVE-2024-1 TBD```", field="body")
        self.assertGreaterEqual(len(errors), 2)


class ValidateReleaseSummaryTests(unittest.TestCase):
    def test_accepts_a_headline_only_summary(self):
        errors = safety.validate_release_summary(
            {"headline": "A focused preview release."}, tag="v1.0.0-preview.1"
        )
        self.assertEqual(errors, [])

    def test_accepts_a_headline_and_body(self):
        errors = safety.validate_release_summary(
            {
                "headline": "A focused preview release.",
                "body": "It adds a new API and fixes a rendering bug.",
            },
            tag="v1.0.0-preview.1",
        )
        self.assertEqual(errors, [])

    def test_rejects_a_non_object(self):
        errors = safety.validate_release_summary("not-an-object", tag="v1.0.0")
        self.assertTrue(any("must be a JSON object" in error for error in errors))

    def test_rejects_a_missing_headline(self):
        errors = safety.validate_release_summary({}, tag="v1.0.0")
        self.assertTrue(any("headline" in error for error in errors))

    def test_rejects_an_unsafe_body_even_with_a_safe_headline(self):
        errors = safety.validate_release_summary(
            {"headline": "A focused release.", "body": "```\ncode\n```"},
            tag="v1.0.0",
        )
        self.assertTrue(any("code fence" in error for error in errors))

    def test_rejects_unknown_fields(self):
        errors = safety.validate_release_summary(
            {"headline": "A focused release.", "extra": "nope"}, tag="v1.0.0"
        )
        self.assertTrue(any("unknown fields" in error for error in errors))

    def test_null_body_is_accepted(self):
        errors = safety.validate_release_summary(
            {"headline": "A focused release.", "body": None}, tag="v1.0.0"
        )
        self.assertEqual(errors, [])


class SafeLoginTests(unittest.TestCase):
    def test_accepts_a_normal_login(self):
        self.assertEqual(safety.safe_login("mattleibow"), "mattleibow")

    def test_accepts_hyphens_and_digits(self):
        self.assertEqual(safety.safe_login("kasperk81"), "kasperk81")
        self.assertEqual(safety.safe_login("a-b-c"), "a-b-c")

    def test_rejects_non_string(self):
        self.assertIsNone(safety.safe_login(None))
        self.assertIsNone(safety.safe_login(123))

    def test_rejects_a_login_starting_with_a_hyphen(self):
        self.assertIsNone(safety.safe_login("-evil"))

    def test_rejects_markdown_injection_attempts(self):
        for value in ("a](https://evil.example)", "a\nb", "a*b*", "@a"):
            with self.subTest(value=value):
                self.assertIsNone(safety.safe_login(value))

    def test_rejects_an_overlong_login(self):
        self.assertIsNone(safety.safe_login("a" * 40))


if __name__ == "__main__":
    unittest.main()
