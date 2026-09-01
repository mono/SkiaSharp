"""The exact-shipment model: one record per exact release tag on a page.

A "shipment" is the deterministic fact record the exact-summary updater keys
its GitHub Release lookup on: an exact tag, the commit it points at, the
PRs delta since the previous tag (in GLOBAL tag order -- a preview's
"previous" shipment may be the prior line's last stable release), and a
compare link. ``collect_shipments`` is a pure function: every git/GitHub
access is injected as a callable so it is fully unit testable without a real
repository or network.
"""

from __future__ import annotations

import re
from typing import Callable, Iterable

from . import common

_SHA256_HEX_RE = re.compile(r"^[0-9a-f]{40}$")
_REQUIRED_FIELDS = (
    "tag",
    "core_version",
    "public_version",
    "channel",
    "label",
    "previous_tag",
    "target_sha",
    "date",
    "changelog_url",
    "prs",
)
def collect_shipments(
    page_version: str,
    all_tags: Iterable[str],
    *,
    tag_date: Callable[[str], str],
    target_sha: Callable[[str], str],
    prs_between: Callable[[str | None, str], list[dict]],
) -> list[dict]:
    """Every exact shipment whose tag core matches ``page_version``.

    ``all_tags`` is every tag in the repository (typically ``git tag -l
    "v*"``); tags outside :data:`common.EXACT_RELEASE_TAG_RE` are ignored.
    The "previous" tag for delta/compare purposes is the immediately
    preceding tag in the GLOBAL sort order across every core version -- not
    just this page's -- exactly like ``release-notes-data.py``'s
    ``collect_preview_milestones`` predecessor lookup, so a page's earliest
    preview correctly compares against the prior line's last release.
    """

    parsed_all = sorted(
        (parsed for tag in all_tags if (parsed := common.parse_tag(tag)) is not None),
        key=lambda parsed: parsed.sort_key,
    )
    page_core = common.core_tuple(page_version)
    shipments = []
    for index, item in enumerate(parsed_all):
        if item.core_tuple != page_core:
            continue
        previous = parsed_all[index - 1] if index > 0 else None
        previous_tag = previous.tag if previous is not None else None
        prs = prs_between(previous_tag, item.tag)
        pr_numbers = sorted({
            number
            for pr in prs
            if isinstance(number := pr.get("number"), int)
        })
        shipments.append({
            "tag": item.tag,
            "core_version": item.core,
            "public_version": item.public_version,
            "channel": item.channel_name,
            "label": item.label,
            "previous_tag": previous_tag,
            "target_sha": target_sha(item.tag),
            "date": tag_date(item.tag),
            "changelog_url": (
                "https://github.com/{}/compare/{}...{}".format(
                    common.REPO, previous_tag, item.tag
                )
                if previous_tag else None
            ),
            "prs": pr_numbers,
        })
    return shipments


def validate_shipment(shipment: object) -> list[str]:
    """Structural validation for one shipment record.

    Applied both when a freshly generated shipment is about to be written to
    data.json and, defensively, when the updater reads a committed data.json
    back -- the same guard rejects a corrupted or hand-edited record either
    way, matching "exact-tag/version association" as a first-class check
    rather than trusting a JSON file's shape.
    """

    if not isinstance(shipment, dict):
        return ["shipment must be a JSON object"]
    errors = [
        "shipment missing required field {!r}".format(field)
        for field in _REQUIRED_FIELDS
        if field not in shipment
    ]
    if errors:
        return errors
    tag = shipment["tag"]
    parsed = common.parse_tag(tag) if isinstance(tag, str) else None
    if parsed is None:
        errors.append("shipment tag {!r} is not an exact release tag".format(tag))
        return errors
    expected = {
        "core_version": parsed.core,
        "public_version": parsed.public_version,
        "channel": parsed.channel_name,
        "label": parsed.label,
    }
    for field, value in expected.items():
        if shipment.get(field) == value:
            continue
        errors.append(
            "shipment {} {} {!r} does not match {!r} derived from its tag".format(
                tag, field, shipment.get(field), value
            )
        )
    target_sha = shipment.get("target_sha")
    if not isinstance(target_sha, str) or not _SHA256_HEX_RE.fullmatch(target_sha):
        errors.append("shipment {} has an invalid target_sha".format(tag))
    prs = shipment.get("prs")
    if not isinstance(prs, list) or not all(isinstance(n, int) for n in prs):
        errors.append("shipment {} prs must be an array of integers".format(tag))
    previous_tag = shipment.get("previous_tag")
    previous = common.parse_tag(previous_tag) if isinstance(previous_tag, str) else None
    if previous_tag is not None and previous is None:
        errors.append("shipment {} previous_tag must be an exact release tag or null".format(tag))
    elif previous is not None and previous.sort_key >= parsed.sort_key:
        errors.append("shipment {} previous_tag must precede its tag".format(tag))
    changelog_url = shipment.get("changelog_url")
    expected_changelog = (
        "https://github.com/{}/compare/{}...{}".format(common.REPO, previous_tag, tag)
        if previous is not None
        else None
    )
    if changelog_url != expected_changelog:
        errors.append(
            "shipment {} changelog_url for previous_tag {!r} is {!r}; expected {!r}".format(
                tag, previous_tag, changelog_url, expected_changelog
            )
        )
    return errors


def validate_shipments(shipments: object) -> list[str]:
    """Validate a whole ``data.json["shipments"]`` array, including uniqueness."""

    if not isinstance(shipments, list):
        return ["shipments must be an array"]
    errors: list[str] = []
    seen: set[str] = set()
    for shipment in shipments:
        errors.extend(validate_shipment(shipment))
        tag = shipment.get("tag") if isinstance(shipment, dict) else None
        if isinstance(tag, str):
            if tag in seen:
                errors.append("duplicate shipment tag {!r}".format(tag))
            seen.add(tag)
    return errors
