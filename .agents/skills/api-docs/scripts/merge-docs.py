#!/usr/bin/env python3
"""
merge-docs.py — Merge agent-filled JSON documentation back into ECMA XML files.

Uses lxml with strip_cdata=False to preserve CDATA sections byte-for-byte.
Only modifies <Docs> block children — structurally cannot touch MemberSignature.

Usage:
  # Merge all filled JSON files back into XML
  python3 merge-docs.py work/ --validate

  # Merge a single JSON file
  python3 merge-docs.py work/SkiaSharp__SKCanvas.json --validate

  # Dry run — show what would change
  python3 merge-docs.py work/ --dry-run
"""

import argparse
import json
import os
import sys
from pathlib import Path

from lxml import etree


def merge_file(json_path: str, dry_run: bool = False, validate: bool = False) -> int:
    """Merge a single JSON file back into its source XML. Returns count of fields updated."""
    with open(json_path) as f:
        data = json.load(f)

    xml_path = data["file"]
    if not os.path.exists(xml_path):
        print(f"  Warning: XML file not found: {xml_path}", file=sys.stderr)
        return 0

    # Parse with CDATA preservation
    parser = etree.XMLParser(strip_cdata=False, remove_blank_text=False)
    tree = etree.parse(xml_path, parser)
    root = tree.getroot()

    # Count signatures before (safety assertion)
    sig_count_before = len(root.findall(".//MemberSignature")) + len(root.findall(".//TypeSignature"))

    updates = 0

    for entry in data.get("entries", []):
        doc_id = entry.get("docId")
        member_type = entry.get("memberType")
        fields = entry.get("fields", {})

        # Skip entries with no filled content
        if not any(v for k, v in fields.items() if k != "params" and k != "typeparams"):
            if "params" not in fields and "typeparams" not in fields:
                continue

        # Find the target Docs block
        docs = None
        if member_type == "type" or doc_id is None:
            docs = root.find("Docs")
        else:
            docs = _find_docs_by_docid(root, doc_id)

        if docs is None:
            if doc_id:
                print(f"  Warning: DocId not found: {doc_id}", file=sys.stderr)
            continue

        # Apply updates
        for field_name in ("summary", "returns", "value", "remarks"):
            if field_name in fields:
                content = fields[field_name]
                if content is not None and "To be added" not in content:
                    elem = docs.find(field_name)
                    if elem is not None:
                        if dry_run:
                            print(f"  Would update {doc_id or 'type'}.{field_name}")
                        else:
                            _set_element_content(elem, content)
                        updates += 1

        # Params
        if "params" in fields:
            for param_name, content in fields["params"].items():
                if content is not None and "To be added" not in content:
                    for param_elem in docs.findall("param"):
                        if param_elem.get("name") == param_name:
                            if dry_run:
                                print(f"  Would update {doc_id or 'type'}.param.{param_name}")
                            else:
                                _set_element_content(param_elem, content)
                            updates += 1
                            break

        # Typeparams
        if "typeparams" in fields:
            for tp_name, content in fields["typeparams"].items():
                if content is not None and "To be added" not in content:
                    for tp_elem in docs.findall("typeparam"):
                        if tp_elem.get("name") == tp_name:
                            if dry_run:
                                print(f"  Would update {doc_id or 'type'}.typeparam.{tp_name}")
                            else:
                                _set_element_content(tp_elem, content)
                            updates += 1
                            break

    if updates == 0:
        return 0

    # Safety assertion: signature count must not change
    sig_count_after = len(root.findall(".//MemberSignature")) + len(root.findall(".//TypeSignature"))
    if sig_count_after != sig_count_before:
        print(f"  FATAL: Signature count changed in {xml_path} "
              f"({sig_count_before} → {sig_count_after}). Aborting.", file=sys.stderr)
        sys.exit(2)

    if not dry_run:
        # Write back preserving CDATA and formatting
        tree.write(xml_path, encoding="utf-8", xml_declaration=False,
                   pretty_print=False)

        if validate:
            # Validate well-formedness
            try:
                etree.parse(xml_path, etree.XMLParser())
            except etree.XMLSyntaxError as e:
                print(f"  ERROR: Written file is malformed: {xml_path}: {e}",
                      file=sys.stderr)
                return -1

    return updates


def _find_docs_by_docid(root, doc_id):
    """Find a <Docs> element by the parent Member's DocId signature."""
    for member in root.iter("Member"):
        for sig in member.findall("MemberSignature"):
            if sig.get("Language") == "DocId" and sig.get("Value") == doc_id:
                return member.find("Docs")
    return None


def _set_element_content(elem, content):
    """Set element content, parsing XML fragments like <see cref="..." />.

    Handles mixed content: plain text + inline XML elements.
    If content is empty string, makes element self-closing.
    """
    if not content or content.strip() == "":
        # Self-closing element
        elem.text = None
        for child in list(elem):
            elem.remove(child)
        return

    # Try parsing as XML fragment
    try:
        fragment = etree.fromstring(
            f"<_wrap_>{content}</_wrap_>",
            parser=etree.XMLParser(strip_cdata=False)
        )
        # Clear existing content
        elem.text = None
        for child in list(elem):
            elem.remove(child)
        # Copy parsed content
        elem.text = fragment.text
        for child in fragment:
            elem.append(child)
    except etree.XMLSyntaxError:
        # Not valid XML — treat as plain text
        for child in list(elem):
            elem.remove(child)
        elem.text = content


def main():
    parser = argparse.ArgumentParser(
        description="Merge agent-filled JSON docs back into ECMA XML files.")
    parser.add_argument("path", help="JSON file or directory of JSON files")
    parser.add_argument("--dry-run", action="store_true",
                        help="Show what would change without writing")
    parser.add_argument("--validate", action="store_true",
                        help="Validate XML well-formedness after writing")
    args = parser.parse_args()

    path = Path(args.path)
    if path.is_file():
        json_files = [path]
    else:
        json_files = sorted(path.glob("*.json"))

    total_updates = 0
    total_files = 0

    for json_file in json_files:
        updates = merge_file(str(json_file), dry_run=args.dry_run,
                             validate=args.validate)
        if updates > 0:
            total_files += 1
            total_updates += updates
            action = "Would update" if args.dry_run else "Updated"
            print(f"  {json_file.name}: {updates} fields")
        elif updates < 0:
            print(f"  {json_file.name}: ERROR — malformed output")

    action = "Would update" if args.dry_run else "Merged"
    print(f"\n{action} {total_updates} fields across {total_files} files")


if __name__ == "__main__":
    main()
