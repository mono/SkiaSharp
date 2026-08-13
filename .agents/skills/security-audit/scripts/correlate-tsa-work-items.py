#!/usr/bin/env python3
"""Attach cached TSA evidence to a security audit and correlate known identifiers."""

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path


IDENTIFIER_PATTERN = re.compile(r"\b(?:CVE-\d{4}-\d+|GHSA-[A-Za-z0-9-]+)\b", re.IGNORECASE)


def normalized(value):
    return re.sub(r"[^a-z0-9]+", " ", value.lower()).strip()


def correlate(report, tsa):
    findings = report.get("findings", [])
    cg_alerts = report.get("cgAlerts", {}).get("alerts", [])
    cve_to_dependency = {}
    dependency_names = set()
    for finding in findings:
        dependency = finding.get("dependency", "")
        if dependency:
            dependency_names.add(dependency)
        for cve in finding.get("cves", []) + finding.get("nonChromeCves", []):
            if cve.get("id"):
                cve_to_dependency[cve["id"].upper()] = dependency

    cg_by_id = {alert.get("id", "").upper(): alert for alert in cg_alerts if alert.get("id")}
    correlated_count = 0
    finding_count = 0
    cg_count = 0

    for item in tsa.get("items", []):
        haystack = " ".join([
            item.get("title", ""),
            " ".join(item.get("tags", [])),
            item.get("tool", ""),
            " ".join(item.get("ruleIds", [])),
        ])
        identifiers = {identifier.upper() for identifier in IDENTIFIER_PATTERN.findall(haystack)}
        finding_dependencies = {
            cve_to_dependency[identifier]
            for identifier in identifiers
            if identifier in cve_to_dependency
        }
        cg_ids = {identifier for identifier in identifiers if identifier in cg_by_id}
        methods = []
        if finding_dependencies or cg_ids:
            methods.append("exact_identifier")

        normalized_haystack = f" {normalized(haystack)} "
        for dependency in dependency_names:
            token = normalized(dependency)
            if len(token) >= 4 and f" {token} " in normalized_haystack:
                finding_dependencies.add(dependency)
                methods.append("dependency_text")

        for alert_id, alert in cg_by_id.items():
            component = normalized(re.sub(r"\s+\d[\w.\-+:~]*$", "", alert.get("component", "")))
            if len(component) >= 4 and f" {component} " in normalized_haystack:
                cg_ids.add(alert_id)
                methods.append("component_text")

        matched = bool(finding_dependencies or cg_ids)
        item["correlation"] = {
            "status": "matched" if matched else "unmatched",
            "findingDependencies": sorted(finding_dependencies),
            "cgAlertIds": sorted(cg_ids),
            "methods": sorted(set(methods)),
        }
        correlated_count += matched
        finding_count += bool(finding_dependencies)
        cg_count += bool(cg_ids)

    summary = tsa.setdefault("summary", {})
    summary["correlated"] = correlated_count
    summary["unmatched"] = len(tsa.get("items", [])) - correlated_count
    summary["withFindingMatches"] = finding_count
    summary["withCgMatches"] = cg_count

    group_by_key = {group["key"]: group for group in tsa.get("groups", [])}
    for group in group_by_key.values():
        group["hasActiveHistory"] = bool(group.get("activeIds") and group.get("historicalIds"))
    summary["activeGroupsWithHistory"] = sum(group.get("hasActiveHistory", False) for group in group_by_key.values())
    summary["historicalOnlyGroups"] = sum(
        bool(group.get("historicalIds")) and not group.get("activeIds")
        for group in group_by_key.values()
    )

    report["tsaWorkItems"] = tsa
    return report


def main():
    parser = argparse.ArgumentParser(description="Correlate TSA work items into a security audit report")
    parser.add_argument("--report", required=True, help="Security audit JSON to update")
    parser.add_argument("--tsa-cache", required=True, help="Output from query-tsa-work-items.py")
    parser.add_argument("--output", help="Output path; defaults to updating --report")
    args = parser.parse_args()

    report_path = Path(args.report)
    output_path = Path(args.output) if args.output else report_path
    with open(report_path, encoding="utf-8") as handle:
        report = json.load(handle)
    with open(args.tsa_cache, encoding="utf-8") as handle:
        tsa = json.load(handle)

    if tsa.get("queryStatus") != "success":
        print(
            f"ERROR: TSA query status is {tsa.get('queryStatus', 'unknown')}: "
            f"{tsa.get('error', 'no error detail')}",
            file=sys.stderr,
        )
        return 1

    correlate(report, tsa)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as handle:
        json.dump(report, handle, indent=2, ensure_ascii=False)

    summary = report["tsaWorkItems"]["summary"]
    print(
        f"[TSA] Correlated {summary['correlated']} of {summary['total']} items; "
        f"{summary['unmatched']} retained as unmatched evidence"
    )
    print(f"[TSA] Report written to: {output_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
