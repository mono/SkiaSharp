#!/usr/bin/env python3
"""Query the legacy TSA Azure Boards work items for the SkiaSharp codebase."""

import argparse
import json
import re
import subprocess
import sys
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import quote


ORG = "https://dev.azure.com/devdiv"
PROJECT = "DevDiv"
CODEBASE_TAG = "TSA-skiasharp.skiasharp_main"
ACTIVE_STATES = {"active", "approved", "committed", "doing", "in progress", "new", "proposed"}
FIELDS = [
    "System.Id",
    "System.Title",
    "System.State",
    "System.WorkItemType",
    "Microsoft.VSTS.Common.Severity",
    "Microsoft.VSTS.Common.Priority",
    "System.Tags",
    "System.AreaPath",
    "System.IterationPath",
    "System.AssignedTo",
    "System.CreatedDate",
    "System.ChangedDate",
]


def iso_now():
    return datetime.now(timezone.utc).isoformat()


def identity_name(value):
    if isinstance(value, dict):
        return value.get("displayName") or value.get("uniqueName")
    return value


def split_tags(raw_tags):
    return sorted({tag.strip() for tag in (raw_tags or "").split(";") if tag.strip()})


def derive_tool_and_rules(tags, title):
    tool_rules = []
    for tag in tags:
        match = re.fullmatch(r"TSA-([A-Za-z0-9_.]+)-([A-Za-z]+[A-Za-z0-9_.-]*)", tag)
        if not match or tag.startswith(f"{CODEBASE_TAG}-"):
            continue
        tool, rule = match.groups()
        if tool.lower() not in {"security", "compliance"}:
            tool_rules.append((tool, rule))

    if tool_rules:
        tools = sorted({tool for tool, _ in tool_rules})
        rules = sorted({rule for _, rule in tool_rules})
        return tools[0], rules

    route_tags = [tag for tag in tags if tag.startswith(f"{CODEBASE_TAG}-")]
    for tag in route_tags:
        match = re.search(r"-(.+?)SARIF-", tag)
        if match:
            return match.group(1), []

    title_match = re.match(r"\[([^]:]+)(?::[^\]]+)?\]", title or "")
    return (title_match.group(1) if title_match else "Unknown"), []


def derive_category(tags):
    if "TSA-Security" in tags:
        return "Security"
    if "TSA-Compliance" in tags:
        return "Compliance"
    return "Tooling"


def transform_rows(rows, queried_at, cache_file):
    if not rows:
        raise ValueError(
            f"Azure Boards returned no items for {CODEBASE_TAG}; "
            "treating this as incomplete evidence rather than an empty success"
        )

    items = []
    groups = defaultdict(lambda: {"activeIds": [], "historicalIds": []})

    for row in rows:
        fields = row.get("fields", {})
        item_id = fields.get("System.Id", row.get("id"))
        title = fields.get("System.Title", "")
        state = fields.get("System.State", "Unknown")
        activity = "active" if state.lower() in ACTIVE_STATES else "historical"
        tags = split_tags(fields.get("System.Tags"))
        tool, rule_ids = derive_tool_and_rules(tags, title)
        dedup_key = f"{tool}:{','.join(rule_ids) if rule_ids else title}"
        url = f"{ORG}/{PROJECT}/_workitems/edit/{item_id}"

        item = {
            "id": item_id,
            "title": title,
            "state": state,
            "activity": activity,
            "workItemType": fields.get("System.WorkItemType", "Unknown"),
            "severity": fields.get("Microsoft.VSTS.Common.Severity"),
            "priority": fields.get("Microsoft.VSTS.Common.Priority"),
            "tags": tags,
            "areaPath": fields.get("System.AreaPath"),
            "iterationPath": fields.get("System.IterationPath"),
            "assignedTo": identity_name(fields.get("System.AssignedTo")),
            "createdDate": fields.get("System.CreatedDate"),
            "changedDate": fields.get("System.ChangedDate"),
            "url": url,
            "tool": tool,
            "ruleIds": rule_ids,
            "tsaCategory": derive_category(tags),
            "dedupKey": dedup_key,
            "rawFields": fields,
        }
        items.append(item)
        groups[dedup_key][f"{activity}Ids"].append(item_id)

    items.sort(key=lambda item: (item.get("changedDate") or "", item["id"]), reverse=True)
    items.sort(key=lambda item: item["activity"] != "active")
    by_state = Counter(item["state"] for item in items)
    by_category = Counter(item["tsaCategory"] for item in items)
    by_tool = Counter(item["tool"] for item in items)

    group_records = []
    for key, ids in sorted(groups.items()):
        tool, _, rules = key.partition(":")
        group_records.append({
            "key": key,
            "tool": tool,
            "ruleIds": [rule for rule in rules.split(",") if rule],
            "activeIds": sorted(ids["activeIds"]),
            "historicalIds": sorted(ids["historicalIds"]),
        })

    wiql = (
        f"SELECT {', '.join(f'[{field}]' for field in FIELDS)} FROM WorkItems "
        f"WHERE [System.TeamProject] = '{PROJECT}' "
        f"AND [System.Tags] CONTAINS '{CODEBASE_TAG}' "
        "ORDER BY [System.ChangedDate] DESC"
    )
    active_count = sum(item["activity"] == "active" for item in items)
    return {
        "queryStatus": "success",
        "queriedAt": queried_at,
        "organization": ORG,
        "project": PROJECT,
        "codebaseTag": CODEBASE_TAG,
        "wiql": wiql,
        "portalSearchUrl": (
            f"https://almsearch.dev.azure.com/devdiv/{PROJECT}/_search"
            f"?type=workitem&text={quote(CODEBASE_TAG)}"
        ),
        "cacheFile": cache_file,
        "summary": {
            "total": len(items),
            "active": active_count,
            "historical": len(items) - active_count,
            "byState": dict(sorted(by_state.items())),
            "byCategory": dict(sorted(by_category.items())),
            "byTool": dict(sorted(by_tool.items())),
            "correlated": 0,
            "unmatched": len(items),
        },
        "groups": group_records,
        "items": items,
    }


def error_result(message, queried_at, cache_file):
    return {
        "queryStatus": "error",
        "queriedAt": queried_at,
        "organization": ORG,
        "project": PROJECT,
        "codebaseTag": CODEBASE_TAG,
        "wiql": None,
        "portalSearchUrl": (
            f"https://almsearch.dev.azure.com/devdiv/{PROJECT}/_search"
            f"?type=workitem&text={quote(CODEBASE_TAG)}"
        ),
        "cacheFile": cache_file,
        "error": message,
        "summary": {
            "total": 0,
            "active": 0,
            "historical": 0,
            "byState": {},
            "byCategory": {},
            "byTool": {},
            "correlated": 0,
            "unmatched": 0,
        },
        "groups": [],
        "items": [],
    }


def query_rows():
    wiql = (
        f"SELECT {', '.join(f'[{field}]' for field in FIELDS)} FROM WorkItems "
        f"WHERE [System.TeamProject] = '{PROJECT}' "
        f"AND [System.Tags] CONTAINS '{CODEBASE_TAG}' "
        "ORDER BY [System.ChangedDate] DESC"
    )
    command = [
        "az", "boards", "query",
        "--org", ORG,
        "--project", PROJECT,
        "--wiql", wiql,
        "--output", "json",
    ]
    result = subprocess.run(command, capture_output=True, text=True, timeout=180)
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "az boards query failed")
    data = json.loads(result.stdout)
    if not isinstance(data, list):
        raise RuntimeError("Azure Boards query returned a non-array response")
    return data


def main():
    parser = argparse.ArgumentParser(description="Query SkiaSharp TSA work items from DevDiv Azure Boards")
    parser.add_argument("--output", "-o", required=True, help="Cache output path")
    parser.add_argument("--input-json", help="Replay raw az boards query JSON instead of calling Azure Boards")
    args = parser.parse_args()

    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    queried_at = iso_now()

    try:
        if args.input_json:
            with open(args.input_json, encoding="utf-8") as handle:
                rows = json.load(handle)
        else:
            rows = query_rows()
        output = transform_rows(rows, queried_at, str(output_path))
        exit_code = 0
    except Exception as exc:
        output = error_result(str(exc), queried_at, str(output_path))
        exit_code = 1

    with open(output_path, "w", encoding="utf-8") as handle:
        json.dump(output, handle, indent=2, ensure_ascii=False)

    summary = output["summary"]
    print(
        f"[TSA] {output['queryStatus']}: {summary['total']} items "
        f"({summary['active']} active, {summary['historical']} historical)"
    )
    print(f"[TSA] JSON written to: {output_path}")
    if output.get("error"):
        print(f"[TSA] ERROR: {output['error']}", file=sys.stderr)
    return exit_code


if __name__ == "__main__":
    sys.exit(main())
