"""Safety guards for reviewed release-summary prose.

Ported from the retired ``release-publish`` skill's GitHub Release "teaser"
guards (``assemble_release_body`` in
``.agents/skills/release-publish/scripts/release_github.py`` on ``main``):
no code fence, no CVE/security/vulnerability claims, no unwritten
placeholder, and a real plain-language opening sentence. Two rules are new
here, specific to the managed-marker design the teaser never had: prose must
not itself contain any managed marker or ``<!-- RELEASE_LINKS -->`` sentinel
(an untrusted PR title or a compromised prose.json entry could otherwise
smuggle a marker byte sequence into the body and corrupt the region
boundaries the updater trusts), and it must not contain a raw HTML comment at
all (the only place a body legitimately carries one is a marker, which is
already covered).
"""

from __future__ import annotations

import re

from . import common

_gh = common.import_release_github()

MANAGED_MARKERS = (
    _gh.SUMMARY_START_MARKER,
    _gh.SUMMARY_END_MARKER,
    _gh.GENERATED_START_MARKER,
    _gh.GENERATED_END_MARKER,
)
RELEASE_LINKS_MARKER = "<!-- RELEASE_LINKS -->"

# Ported verbatim in spirit from the retired teaser's
# ``assemble_release_body`` check: never let reviewed prose name a CVE or
# call anything a security fix -- we deliberately never signal which bundled
# component was vulnerable.
SECURITY_CLAIM_RE = re.compile(
    r"\b(?:CVE-\d|security (?:fix|release|patch|advisory)|vulnerabilit)",
    re.IGNORECASE,
)

_PLACEHOLDER_PATTERNS = (
    re.compile(r"replace this", re.IGNORECASE),
    re.compile(r"\btbd\b", re.IGNORECASE),
    re.compile(r"\btodo\b", re.IGNORECASE),
    re.compile(r"lorem ipsum", re.IGNORECASE),
    re.compile(r"^(?:no|none|n/a)\.?$", re.IGNORECASE),
)

# GitHub login grammar: alphanumeric/hyphen, max 39 characters, never starting
# with a hyphen. Used before interpolating any login into a rendered summary
# (an untrusted PR author/contributor login is still just JSON text by the
# time it reaches here) so a malformed login can never inject Markdown.
LOGIN_RE = re.compile(r"^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$")

HEADLINE_WORD_CAP = 40
BODY_WORD_CAP = 120


def _words(text: str) -> int:
    return len(re.findall(r"\S+", text))


def validate_prose_text(text: object, *, field: str, max_words: int | None = None) -> list[str]:
    """Guard one prose string (a summary headline or body) against every
    known unsafe pattern. Returns a list of human-readable errors; an empty
    list means the text is safe to render."""

    if not isinstance(text, str):
        return ["{} must be a string".format(field)]
    stripped = text.strip()
    if not stripped:
        return ["{} is empty".format(field)]
    errors = []
    if "```" in stripped:
        errors.append("{} must not contain a code fence".format(field))
    for marker in MANAGED_MARKERS + (RELEASE_LINKS_MARKER,):
        if marker in stripped:
            errors.append(
                "{} must not contain the managed marker {!r}".format(field, marker)
            )
    if "<!--" in stripped or "-->" in stripped:
        errors.append("{} must not contain an HTML comment".format(field))
    if SECURITY_CLAIM_RE.search(stripped):
        errors.append(
            "{} must not advertise security or vulnerability details".format(field)
        )
    if any(pattern.search(stripped) for pattern in _PLACEHOLDER_PATTERNS):
        errors.append("{} looks like an unwritten placeholder".format(field))
    first_line = stripped.splitlines()[0].strip()
    if first_line.startswith(("#", "<", "*", "-", "|")):
        errors.append(
            "{} must start with a plain-language sentence, not a heading, list, "
            "or table marker".format(field)
        )
    if max_words is not None:
        word_count = _words(stripped)
        if word_count > max_words:
            errors.append(
                "{} is {} words (cap {})".format(field, word_count, max_words)
            )
    return errors


def validate_release_summary(summary: object, *, tag: str) -> list[str]:
    """Validate one ``prose.json["release_summaries"][tag]`` entry."""

    if not isinstance(summary, dict):
        return ["{} release_summaries entry must be a JSON object".format(tag)]
    errors = list(
        validate_prose_text(
            summary.get("headline"),
            field="{} release_summaries.headline".format(tag),
            max_words=HEADLINE_WORD_CAP,
        )
    )
    body = summary.get("body")
    if body is not None:
        errors.extend(
            validate_prose_text(
                body,
                field="{} release_summaries.body".format(tag),
                max_words=BODY_WORD_CAP,
            )
        )
    extra_keys = sorted(set(summary) - {"headline", "body"})
    if extra_keys:
        errors.append(
            "{} release_summaries entry has unknown fields: {}".format(tag, extra_keys)
        )
    return errors


def safe_login(login: object) -> str | None:
    """Return ``login`` unchanged if it is a syntactically valid GitHub
    login, else ``None``. Never interpolate an unvalidated login into a
    rendered summary."""

    if isinstance(login, str) and LOGIN_RE.fullmatch(login):
        return login
    return None
