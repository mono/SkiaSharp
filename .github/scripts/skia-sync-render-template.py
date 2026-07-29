#!/usr/bin/env python3

import json
import re
import sys
from pathlib import Path


def main() -> int:
    if len(sys.argv) != 4:
        print("usage: skia-sync-render-template.py TEMPLATE VALUES_JSON OUTPUT", file=sys.stderr)
        return 2

    template_path = Path(sys.argv[1])
    values_path = Path(sys.argv[2])
    output_path = Path(sys.argv[3])

    template = template_path.read_text(encoding="utf-8")
    values = json.loads(values_path.read_text(encoding="utf-8"))
    if not isinstance(values, dict) or not all(isinstance(key, str) for key in values):
        raise ValueError("template values must be a JSON object with string keys")

    placeholders = set(re.findall(r"\{\{([A-Z0-9_]+)\}\}", template))
    missing = sorted(placeholders - values.keys())
    if missing:
        raise ValueError(f"missing template values: {', '.join(missing)}")

    def replace(match: re.Match[str]) -> str:
        key = match.group(1)
        value = values[key]
        if not isinstance(value, str):
            raise ValueError(f"template value {key} must be a string")
        return value

    rendered = re.sub(r"\{\{([A-Z0-9_]+)\}\}", replace, template)

    output_path.write_text(rendered.rstrip() + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
