"""A tiny, deterministic, standard-library-only JSON Schema validator.

``scripts/infra/release`` must run on a stock GitHub-hosted runner with
nothing beyond the Python standard library -- no ``pip install`` step and
no network access. The four schema files under ``schemas/`` are all written
by us and intentionally use only a small, fixed subset of JSON Schema
(draft 2020-12) keywords:

* ``type`` (a single type name or a list of them, including ``"null"``)
* ``properties`` / ``required`` / ``additionalProperties`` (object shape)
* ``items`` (a single schema applied to every array element)
* ``const`` / ``enum``
* ``pattern`` (a regular expression, strings only)
* ``format`` (only ``"date-time"`` is used)
* ``minLength`` (strings only)

plus the purely descriptive ``$schema`` / ``$id`` / ``title`` /
``description`` keywords, which carry no validation semantics and are
ignored.

This module implements exactly that subset -- nothing more, and nothing
"best effort". If a schema file under ``schemas/`` is ever edited to use a
keyword or shape this validator does not know about, that is an authoring
bug in the schema (or a real gap in this validator) that must be fixed
before it ships. :func:`validate` therefore raises :class:`SchemaError`
immediately in that case rather than silently ignoring the keyword the way
a partial validator would -- there is no generic/full JSON Schema
implementation backing this, on purpose, so it must never pretend to
support more than it actually checks.
"""

from __future__ import annotations

import re
from dataclasses import dataclass
from typing import Any

_SUPPORTED_KEYWORDS = frozenset(
    {
        "$schema",
        "$id",
        "title",
        "description",
        "type",
        "properties",
        "required",
        "additionalProperties",
        "items",
        "const",
        "enum",
        "pattern",
        "format",
        "minLength",
    }
)

_SUPPORTED_TYPES = frozenset({"object", "array", "string", "boolean", "integer", "number", "null"})

_SUPPORTED_FORMATS = frozenset({"date-time"})

# RFC 3339 date-time, which is what JSON Schema's "date-time" format means.
# Deliberately stricter than "anything datetime.fromisoformat() accepts" so
# a malformed generatedAt timestamp is always caught.
_DATE_TIME_PATTERN = re.compile(
    r"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$"
)


class SchemaError(Exception):
    """A schema file itself uses a keyword or shape this validator does not implement.

    This is always an authoring bug in a checked-in ``schemas/*.json`` file
    (or a real gap in this validator that must be closed deliberately) --
    never something instance data being validated can trigger. It is kept
    as its own exception type, distinct from an ordinary validation
    failure, precisely so it is never mistaken for -- or silently
    swallowed as -- a plan/document validation error.
    """

    def __init__(self, message: str, *, path: tuple[Any, ...]) -> None:
        super().__init__(message)
        self.path = path


@dataclass(frozen=True)
class Issue:
    """One way ``instance`` failed to satisfy a schema, at a specific path."""

    path: tuple[Any, ...]
    message: str

    def formatted_path(self) -> str:
        return ".".join(str(part) for part in self.path) or "(root)"


def validate(instance: Any, schema: dict) -> list[Issue]:
    """Validate ``instance`` against ``schema``; return every issue found.

    Returns an empty list when ``instance`` fully satisfies ``schema``.
    Never raises for problems in the *instance* -- those are always
    reported as :class:`Issue` entries. Only raises :class:`SchemaError`,
    and only when ``schema`` itself uses a keyword, type name, or shape
    this validator does not implement.
    """

    issues: list[Issue] = []
    _validate_node(instance, schema, (), issues)
    return sorted(issues, key=lambda issue: issue.formatted_path())


def _path_label(path: tuple[Any, ...]) -> str:
    return ".".join(str(part) for part in path) or "(root)"


def _check_schema_is_supported(schema: dict, path: tuple[Any, ...]) -> None:
    unsupported = set(schema) - _SUPPORTED_KEYWORDS
    if unsupported:
        raise SchemaError(
            f"schema at {_path_label(path)} uses unsupported JSON Schema "
            f"keyword(s) {sorted(unsupported)}; release_schema.py only "
            "implements a fixed subset -- extend it deliberately before "
            "using a new keyword",
            path=path,
        )

    type_spec = schema.get("type")
    if type_spec is not None:
        types = type_spec if isinstance(type_spec, list) else [type_spec]
        unknown_types = sorted(set(types) - _SUPPORTED_TYPES)
        if unknown_types:
            raise SchemaError(
                f"schema at {_path_label(path)} uses unsupported type(s) {unknown_types}",
                path=path,
            )

    format_spec = schema.get("format")
    if format_spec is not None and format_spec not in _SUPPORTED_FORMATS:
        raise SchemaError(
            f"schema at {_path_label(path)} uses unsupported format {format_spec!r}",
            path=path,
        )

    additional = schema.get("additionalProperties")
    if additional is not None and not isinstance(additional, bool):
        raise SchemaError(
            f"schema at {_path_label(path)} uses a schema-valued "
            "additionalProperties; only boolean true/false is implemented",
            path=path,
        )

    items = schema.get("items")
    if items is not None and not isinstance(items, dict):
        raise SchemaError(
            f"schema at {_path_label(path)} uses tuple-form 'items' (a "
            "list of schemas); only a single schema applied to every "
            "element is implemented",
            path=path,
        )


def _matches_type(instance: Any, type_spec: Any) -> bool:
    types = type_spec if isinstance(type_spec, list) else [type_spec]
    for candidate in types:
        if candidate == "object" and isinstance(instance, dict):
            return True
        if candidate == "array" and isinstance(instance, list):
            return True
        if candidate == "string" and isinstance(instance, str):
            return True
        if candidate == "boolean" and isinstance(instance, bool):
            return True
        if candidate == "integer" and isinstance(instance, int) and not isinstance(instance, bool):
            return True
        if candidate == "number" and isinstance(instance, (int, float)) and not isinstance(instance, bool):
            return True
        if candidate == "null" and instance is None:
            return True
    return False


def _validate_node(instance: Any, schema: dict, path: tuple[Any, ...], issues: list[Issue]) -> None:
    _check_schema_is_supported(schema, path)

    if "const" in schema and instance != schema["const"]:
        issues.append(Issue(path, f"{instance!r} does not equal the required constant {schema['const']!r}"))
        return
    if "enum" in schema and instance not in schema["enum"]:
        issues.append(Issue(path, f"{instance!r} is not one of {schema['enum']!r}"))
        return
    if "type" in schema and not _matches_type(instance, schema["type"]):
        issues.append(Issue(path, f"{instance!r} is not of type {schema['type']!r}"))
        return

    if isinstance(instance, str):
        pattern = schema.get("pattern")
        if pattern is not None and re.search(pattern, instance) is None:
            issues.append(Issue(path, f"{instance!r} does not match pattern {pattern!r}"))
        min_length = schema.get("minLength")
        if min_length is not None and len(instance) < min_length:
            issues.append(Issue(path, f"{instance!r} is shorter than minLength {min_length}"))
        if schema.get("format") == "date-time" and _DATE_TIME_PATTERN.match(instance) is None:
            issues.append(Issue(path, f"{instance!r} is not a valid RFC 3339 date-time"))

    if isinstance(instance, dict):
        for key in schema.get("required", ()):
            if key not in instance:
                issues.append(Issue(path + (key,), "is a required property"))
        properties = schema.get("properties", {})
        for key, subschema in properties.items():
            if key in instance:
                _validate_node(instance[key], subschema, path + (key,), issues)
        if schema.get("additionalProperties") is False:
            for key in sorted(set(instance) - set(properties)):
                issues.append(Issue(path + (key,), "additional properties are not allowed"))

    if isinstance(instance, list):
        item_schema = schema.get("items")
        if item_schema is not None:
            for index, item in enumerate(instance):
                _validate_node(item, item_schema, path + (index,), issues)
