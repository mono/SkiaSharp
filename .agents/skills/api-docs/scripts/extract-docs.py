#!/usr/bin/env python3
"""
extract-docs.py — Extract "To be added." placeholder entries from ECMA XML files to JSON.

Produces one JSON file per XML file containing only the members/fields that need docs.
The agent fills the JSON, then merge-docs.py injects content back into the XML.

Usage:
  # Extract all placeholders from a directory
  python3 extract-docs.py docs/SkiaSharpAPI/ --output work/

  # Extract from a single file
  python3 extract-docs.py docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml --output work/
"""

import argparse
import json
import os
import sys
from pathlib import Path

from lxml import etree


def extract_file(xml_path: str):
    """Extract placeholder docs entries from an ECMA XML file."""
    parser = etree.XMLParser(strip_cdata=False, remove_blank_text=False)
    try:
        tree = etree.parse(xml_path, parser)
    except etree.XMLSyntaxError as e:
        print(f"  Warning: Skipping {xml_path}: {e}", file=sys.stderr)
        return None

    root = tree.getroot()
    type_name = root.get("FullName", root.get("Name", "Unknown"))

    entries = []

    # Type-level docs
    type_docs = root.find("Docs")
    if type_docs is not None:
        type_entry = _extract_docs_block(type_docs, type_name, "type", None)
        if type_entry:
            entries.append(type_entry)

    # Member-level docs
    for member in root.iter("Member"):
        member_name = member.get("MemberName", "?")
        doc_id = None
        cs_sig = None
        member_type = None

        for sig in member.findall("MemberSignature"):
            lang = sig.get("Language")
            if lang == "DocId":
                doc_id = sig.get("Value")
            elif lang == "C#":
                cs_sig = sig.get("Value")

        mt = member.find("MemberType")
        if mt is not None:
            member_type = mt.text

        docs = member.find("Docs")
        if docs is not None:
            entry = _extract_docs_block(docs, type_name, member_type, doc_id)
            if entry:
                entry["memberName"] = member_name
                entry["signature"] = cs_sig
                entries.append(entry)

    if not entries:
        return None

    return {
        "file": xml_path,
        "typeName": type_name,
        "entries": entries,
    }


def _extract_docs_block(docs_elem, type_name, member_type, doc_id):
    """Extract fields with 'To be added.' from a <Docs> block."""
    fields = {}

    for child in docs_elem:
        tag = child.tag
        if tag == "param":
            name = child.get("name", "?")
            text = _get_element_text(child)
            if "To be added" in (text or ""):
                fields.setdefault("params", {})[name] = text
        elif tag == "typeparam":
            name = child.get("name", "?")
            text = _get_element_text(child)
            if "To be added" in (text or ""):
                fields.setdefault("typeparams", {})[name] = text
        elif tag in ("summary", "returns", "value", "remarks"):
            text = _get_element_text(child)
            if text and "To be added" in text:
                fields[tag] = text

    if not fields:
        return None

    entry = {
        "docId": doc_id,
        "memberType": member_type,
        "fields": fields,
    }
    return entry


def _get_element_text(elem):
    """Get the full text content of an element including children."""
    return (elem.text or "") + "".join(
        etree.tostring(child, encoding="unicode") for child in elem
    )


def main():
    parser = argparse.ArgumentParser(
        description="Extract 'To be added.' entries from ECMA XML to JSON.")
    parser.add_argument("path", help="XML file or directory to scan")
    parser.add_argument("--output", "-o", required=True,
                        help="Output directory for JSON files")
    args = parser.parse_args()

    os.makedirs(args.output, exist_ok=True)
    path = Path(args.path)

    if path.is_file():
        xml_files = [path]
    else:
        xml_files = sorted(path.rglob("*.xml"))

    total_entries = 0
    total_files = 0

    for xml_file in xml_files:
        result = extract_file(str(xml_file))
        if result and result["entries"]:
            # Use relative path as JSON filename
            rel = xml_file.relative_to(path) if path.is_dir() else xml_file.name
            json_name = str(rel).replace("/", "__").replace(".xml", ".json")
            json_path = os.path.join(args.output, json_name)

            with open(json_path, "w") as f:
                json.dump(result, f, indent=2)

            n = len(result["entries"])
            total_entries += n
            total_files += 1
            print(f"  {xml_file}: {n} entries")

    print(f"\nExtracted {total_entries} entries from {total_files} files to {args.output}/")


if __name__ == "__main__":
    main()
