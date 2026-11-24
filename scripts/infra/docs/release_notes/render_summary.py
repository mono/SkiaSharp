"""Deterministically render one exact shipment's complete GitHub Release body.

Scripts own every heading, link, and contributor credit; the agent supplies
only ``headline``/``body`` prose strings in ``prose.json["release_summaries"]``.
This mirrors the ``release-notes-render.py`` split for the website page: the
renderer enforces structure and safety, and raises rather than silently
shipping a violation.
"""

from __future__ import annotations

from . import safety


def _shipment_map(data: dict) -> dict[str, dict]:
    shipments = data.get("shipments") or []
    return {
        shipment["tag"]: shipment
        for shipment in shipments
        if isinstance(shipment, dict) and isinstance(shipment.get("tag"), str)
    }


def _append_once(values: list[str], seen: set[str], value: str) -> None:
    key = value.casefold()
    if key not in seen:
        seen.add(key)
        values.append(value)


def _contributor_credits(
    data: dict, shipment: dict
) -> tuple[list[str], list[str], list[str]]:
    """Return exact-shipment humans, first-timers, and automation/AI credits."""
    humans: list[str] = []
    first_time: list[str] = []
    assistance: list[str] = []
    seen_humans: set[str] = set()
    seen_first_time: set[str] = set()
    seen_assistance: set[str] = set()
    for attribution in shipment.get("attributions") or []:
        if not isinstance(attribution, dict):
            raise ValueError("shipment has an invalid attribution")
        display = attribution.get("display")
        kind = attribution.get("kind")
        if not isinstance(display, str) or kind not in ("human", "automation", "ai"):
            raise ValueError("shipment has an invalid attribution")
        if (
            display.startswith("@")
            and safety.safe_login(display[1:]) is None
        ) or (
            not display.startswith("@")
            and safety.safe_display_label(display) is None
        ):
            raise ValueError("shipment has an unsafe attribution")
        if kind == "human":
            _append_once(humans, seen_humans, display)
            if attribution.get("first_time") is True:
                _append_once(first_time, seen_first_time, display)
        else:
            _append_once(assistance, seen_assistance, display)
    return humans, first_time, assistance


def render_github_release_summary(data: dict, prose: dict, tag: str) -> str:
    """Render the complete canonical Markdown for exact tag ``tag``.

    Raises ``KeyError`` when ``tag`` has no shipment fact or no reviewed
    summary yet (both are legitimate "not ready" states the caller should
    treat as "skip this tag", not a crash) and ``ValueError`` when the
    reviewed prose fails a safety check.
    """

    shipment = _shipment_map(data).get(tag)
    if shipment is None:
        raise KeyError("{} has no exact shipment facts in this data.json".format(tag))
    summaries = (prose or {}).get("release_summaries") or {}
    summary = summaries.get(tag)
    if summary is None:
        raise KeyError("{} has no release_summaries entry in this prose.json".format(tag))
    errors = safety.validate_release_summary(summary, tag=tag)
    if errors:
        raise ValueError("; ".join(errors))

    headline = summary["headline"].strip()
    lines = ["**{}** \u2014 {}".format(shipment["label"], headline)]
    body = (summary.get("body") or "").strip()
    if body:
        lines += ["", body]
    lines += ["", safety.RELEASE_LINKS_MARKER]
    contributors, first_time, assistance = _contributor_credits(data, shipment)
    if contributors:
        lines += [
            "",
            "\U0001F465 Contributors: {}.".format(", ".join(contributors)),
        ]
    if first_time:
        lines += [
            "",
            "\U0001F389 First-time contributors: {}.".format(", ".join(first_time)),
        ]
    if assistance:
        lines += [
            "",
            "\U0001F916 Automation and AI assistance: {}.".format(
                ", ".join(assistance)
            ),
        ]
    return "\n".join(lines) + "\n"
