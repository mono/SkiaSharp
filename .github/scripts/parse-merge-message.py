#!/usr/bin/env python3
"""Safely extract a merge API title and body from a merge-message comment."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path


MAX_COMMENT_BYTES = 65_536
MAX_SUBJECT_LENGTH = 200
TEXT_FENCE_RE = re.compile(r"^```text[ \t]*\n(.*?)^```[ \t]*$", re.MULTILINE | re.DOTALL)


class MessageValidationError(ValueError):
    """The generated comment is not an unambiguous merge message."""


def parse_merge_message(comment: str) -> dict[str, str]:
    if not isinstance(comment, str) or not comment:
        raise MessageValidationError("Comment body must be nonempty text.")
    if len(comment.encode("utf-8")) > MAX_COMMENT_BYTES:
        raise MessageValidationError("Comment body exceeds the size limit.")
    matches = list(TEXT_FENCE_RE.finditer(comment))
    if len(matches) != 1:
        raise MessageValidationError("Expected exactly one fenced text block.")
    if len(re.findall(r"^```[^\n]*$", comment, re.MULTILINE)) != 2:
        raise MessageValidationError("The comment must not contain additional fenced blocks.")
    match = matches[0]
    outside = comment[: match.start()] + comment[match.end() :]
    if re.search(r"(?mi)^Missing context:", outside):
        raise MessageValidationError("Missing context must not appear outside the fenced message.")
    message = match.group(1).rstrip("\n")
    if not message:
        raise MessageValidationError("The fenced message is empty.")
    subject, separator, body = message.partition("\n")
    if not subject.strip() or subject != subject.strip():
        raise MessageValidationError("The subject must be a nonempty first line.")
    if len(subject) > MAX_SUBJECT_LENGTH:
        raise MessageValidationError("The subject exceeds the size limit.")
    if "\x00" in message:
        raise MessageValidationError("The message contains a NUL byte.")
    return {"subject": subject, "body": body if separator else ""}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", type=Path, required=True)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    try:
        result = parse_merge_message(args.input.read_text(encoding="utf-8"))
        encoded = json.dumps(result, ensure_ascii=False)
        if args.output:
            args.output.write_text(encoded + "\n", encoding="utf-8")
        else:
            print(encoded)
    except (MessageValidationError, OSError) as error:
        print(f"parse-merge-message: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
