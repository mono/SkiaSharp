"""Deterministically render one exact shipment's reviewed GitHub Release summary.

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


def _contributor_credits(data: dict, shipment: dict) -> list[str]:
    """Community contributors credited with @handles for this exact shipment.

    Derived entirely from data.json's already-vetted ``contributors`` roster
    (which already excludes the maintainer and bot accounts) intersected with
    this shipment's own PR numbers -- never from agent prose, so a credit can
    never be invented or omitted by the polish step.
    """

    pr_numbers = set(shipment.get("prs") or [])
    credited = []
    for contributor in data.get("contributors") or []:
        if not isinstance(contributor, dict):
            continue
        login = safety.safe_login(contributor.get("login"))
        if login is None:
            continue
        prs = {n for n in (contributor.get("prs") or []) if isinstance(n, int)}
        if prs & pr_numbers:
            credited.append(login)
    return sorted(credited)


def render_github_release_summary(data: dict, prose: dict, tag: str) -> str:
    """Render the managed-summary Markdown for exact tag ``tag``.

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
    credits = _contributor_credits(data, shipment)
    if credits:
        lines += [
            "",
            "Thanks to our contributors: " + ", ".join("@{}".format(login) for login in credits),
        ]
    lines += ["", safety.RELEASE_LINKS_MARKER]
    return "\n".join(lines) + "\n"
