"""Exact-shipment release-summary model and GitHub Release updater.

This package turns deterministic release facts (git tags and PR deltas) into a
small ``shipments`` list per release page, accepts reviewed
``release_summaries`` prose for each exact shipment tag, and renders those
facts into the complete canonical GitHub Release body.

Design rules that every module here follows:

* Scripts own structure — tags, links, headings, and contributor attribution
  are always computed here, never authored by the agent.
* The agent (AI) is limited to short prose strings (a headline and an
  optional body per shipment); everything else is deterministic.
* An unmarked published release is adopted by replacing its existing body;
  legacy generated-notes markers are migration input only. Pages whose data
  predates this format are skipped.
* This package solely owns the managed release-body markers and helpers.
"""

from __future__ import annotations
