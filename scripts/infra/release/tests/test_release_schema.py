from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_schema as schema


class TypeKeywordTests(unittest.TestCase):
    def test_accepts_matching_single_type(self):
        self.assertEqual(schema.validate("hello", {"type": "string"}), [])

    def test_rejects_mismatched_single_type(self):
        issues = schema.validate(42, {"type": "string"})
        self.assertEqual(len(issues), 1)
        self.assertIn("not of type", issues[0].message)

    def test_accepts_either_type_in_a_list(self):
        self.assertEqual(schema.validate(None, {"type": ["string", "null"]}), [])
        self.assertEqual(schema.validate("x", {"type": ["string", "null"]}), [])

    def test_rejects_value_not_in_type_list(self):
        issues = schema.validate(1, {"type": ["string", "null"]})
        self.assertEqual(len(issues), 1)

    def test_boolean_is_not_an_integer(self):
        # bool is a subclass of int in Python; JSON Schema treats them as
        # distinct types, so a bool must not satisfy {"type": "integer"}.
        issues = schema.validate(True, {"type": "integer"})
        self.assertEqual(len(issues), 1)

    def test_integer_is_not_a_boolean(self):
        issues = schema.validate(1, {"type": "boolean"})
        self.assertEqual(len(issues), 1)


class ConstAndEnumTests(unittest.TestCase):
    def test_const_accepts_exact_match(self):
        self.assertEqual(schema.validate(1, {"const": 1}), [])

    def test_const_rejects_mismatch(self):
        issues = schema.validate(2, {"const": 1})
        self.assertEqual(len(issues), 1)
        self.assertIn("does not equal the required constant", issues[0].message)

    def test_enum_accepts_member(self):
        self.assertEqual(schema.validate("b", {"enum": ["a", "b", "c"]}), [])

    def test_enum_rejects_non_member(self):
        issues = schema.validate("z", {"enum": ["a", "b", "c"]})
        self.assertEqual(len(issues), 1)
        self.assertIn("is not one of", issues[0].message)


class StringKeywordTests(unittest.TestCase):
    def test_pattern_accepts_match(self):
        self.assertEqual(schema.validate("abc123", {"pattern": r"^[a-z]+\d+$"}), [])

    def test_pattern_rejects_mismatch(self):
        issues = schema.validate("ABC", {"pattern": r"^[a-z]+$"})
        self.assertEqual(len(issues), 1)
        self.assertIn("does not match pattern", issues[0].message)

    def test_pattern_is_ignored_for_non_string_types(self):
        # Mirrors JSON Schema semantics: "pattern" only constrains string
        # instances, so a null value with type ["string", "null"] and a
        # pattern must not be rejected just because None can't match a regex.
        node = {"type": ["string", "null"], "pattern": r"^\d+$"}
        self.assertEqual(schema.validate(None, node), [])

    def test_min_length_accepts_long_enough_string(self):
        self.assertEqual(schema.validate("hello", {"minLength": 1}), [])

    def test_min_length_rejects_empty_string(self):
        issues = schema.validate("", {"minLength": 1})
        self.assertEqual(len(issues), 1)
        self.assertIn("shorter than minLength", issues[0].message)

    def test_date_time_format_accepts_zulu_timestamp(self):
        node = {"type": "string", "format": "date-time"}
        self.assertEqual(schema.validate("2024-01-02T03:04:05Z", node), [])

    def test_date_time_format_accepts_offset_timestamp(self):
        node = {"type": "string", "format": "date-time"}
        self.assertEqual(schema.validate("2024-01-02T03:04:05+02:00", node), [])

    def test_date_time_format_rejects_bare_date(self):
        node = {"type": "string", "format": "date-time"}
        issues = schema.validate("2024-01-02", node)
        self.assertEqual(len(issues), 1)
        self.assertIn("not a valid RFC 3339 date-time", issues[0].message)


class ObjectKeywordTests(unittest.TestCase):
    def test_required_property_missing_is_reported_at_its_own_path(self):
        node = {"type": "object", "required": ["value"]}
        issues = schema.validate({}, node)
        self.assertEqual(len(issues), 1)
        self.assertEqual(issues[0].formatted_path(), "value")
        self.assertIn("required property", issues[0].message)

    def test_nested_property_is_validated_recursively(self):
        node = {
            "type": "object",
            "properties": {"inner": {"type": "object", "required": ["value"]}},
        }
        issues = schema.validate({"inner": {}}, node)
        self.assertEqual(len(issues), 1)
        self.assertEqual(issues[0].formatted_path(), "inner.value")

    def test_additional_properties_true_allows_extra_keys(self):
        node = {"type": "object", "properties": {"a": {"type": "string"}}, "additionalProperties": True}
        self.assertEqual(schema.validate({"a": "x", "b": "extra"}, node), [])

    def test_additional_properties_default_allows_extra_keys(self):
        node = {"type": "object", "properties": {"a": {"type": "string"}}}
        self.assertEqual(schema.validate({"a": "x", "b": "extra"}, node), [])

    def test_additional_properties_false_rejects_extra_keys(self):
        node = {
            "type": "object",
            "properties": {"a": {"type": "string"}},
            "additionalProperties": False,
        }
        issues = schema.validate({"a": "x", "b": "extra"}, node)
        self.assertEqual(len(issues), 1)
        self.assertEqual(issues[0].formatted_path(), "b")


class ArrayKeywordTests(unittest.TestCase):
    def test_items_validates_every_element(self):
        node = {"type": "array", "items": {"type": "string"}}
        self.assertEqual(schema.validate(["a", "b"], node), [])

    def test_items_reports_each_bad_element_at_its_index(self):
        node = {"type": "array", "items": {"type": "string"}}
        issues = schema.validate(["a", 1, "c", 2], node)
        self.assertEqual([issue.formatted_path() for issue in issues], ["1", "3"])

    def test_empty_array_is_valid_regardless_of_items(self):
        node = {"type": "array", "items": {"type": "string"}}
        self.assertEqual(schema.validate([], node), [])


class DeterministicOrderingTests(unittest.TestCase):
    def test_issues_are_sorted_by_path_regardless_of_schema_key_order(self):
        node = {
            "type": "object",
            "required": ["zeta", "alpha", "mid"],
        }
        issues = schema.validate({}, node)
        self.assertEqual(
            [issue.formatted_path() for issue in issues], ["alpha", "mid", "zeta"]
        )


class UnsupportedSchemaKeywordTests(unittest.TestCase):
    """A schema file using a keyword/shape this validator does not implement
    is always an authoring bug that must fail loudly, not be silently
    ignored -- see release_schema.py's module docstring."""

    def test_unknown_keyword_raises_schema_error(self):
        with self.assertRaises(schema.SchemaError):
            schema.validate("x", {"type": "string", "oneOf": [{"const": "x"}]})

    def test_unknown_type_name_raises_schema_error(self):
        with self.assertRaises(schema.SchemaError):
            schema.validate(1, {"type": "float"})

    def test_unsupported_format_raises_schema_error(self):
        with self.assertRaises(schema.SchemaError):
            schema.validate("x", {"type": "string", "format": "uri"})

    def test_schema_valued_additional_properties_raises_schema_error(self):
        with self.assertRaises(schema.SchemaError):
            schema.validate({}, {"type": "object", "additionalProperties": {"type": "string"}})

    def test_tuple_form_items_raises_schema_error(self):
        with self.assertRaises(schema.SchemaError):
            schema.validate([1, "x"], {"type": "array", "items": [{"type": "integer"}, {"type": "string"}]})

    def test_unknown_keyword_inside_nested_property_is_detected(self):
        node = {
            "type": "object",
            "properties": {"inner": {"type": "string", "unsupportedKeyword": True}},
        }
        with self.assertRaises(schema.SchemaError):
            schema.validate({"inner": "x"}, node)


class RealSchemaFilesAreFullySupportedTests(unittest.TestCase):
    """Every checked-in schemas/*.json file must only use keywords this
    validator implements -- exercised end-to-end via release_common so a
    regression here is caught the same way a real caller would hit it."""

    def test_every_schema_file_validates_without_schema_error(self):
        import release_common as common

        for schema_name in [
            "prepare-plan.schema.json",
            "finish-plan.schema.json",
            "plan-summary.schema.json",
            "result-envelope.schema.json",
        ]:
            loaded = common.load_schema(schema_name)
            # An empty object always fails required-field checks (a real
            # ValidationError-shaped result), but must never raise
            # SchemaError -- that would mean the schema itself uses
            # something this validator does not implement.
            issues = schema.validate({}, loaded)
            self.assertTrue(issues, f"{schema_name} unexpectedly accepted {{}}")


if __name__ == "__main__":
    unittest.main()
