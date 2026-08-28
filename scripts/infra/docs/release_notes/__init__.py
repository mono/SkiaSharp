"""Exact-shipment release-summary model and GitHub Release updater.

This package is the narrow, fresh-ported core of the "exact release summary"
feature: it turns the deterministic facts release-notes-data.py already
collects (git tags, PR deltas) into a small ``shipments`` list per release
page, lets the release-notes skill attach reviewed ``release_summaries``
prose to each exact shipment tag, and then deterministically folds that prose
into the managed summary region of the matching GitHub Release body.

Design rules that every module here follows:

* Scripts own structure — tags, links, headings, and contributor attribution
  are always computed here, never authored by the agent.
* The agent (AI) is limited to short prose strings (a headline and an
  optional body per shipment); everything else is deterministic.
* Historical releases without the managed markers, and pages whose data.json
  predates this format, are safely skipped rather than rewritten.
* The updater never redefines the managed markers itself — it imports them
  from ``scripts/infra/release/release_github.py``, the single source of
  truth Finish uses to compose the initial release body, so the two paths
  can never diverge on marker bytes.
"""

from __future__ import annotations
