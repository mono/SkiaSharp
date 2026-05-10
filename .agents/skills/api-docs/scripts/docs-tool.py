#!/usr/bin/env python3
"""
docs-tool.py — Extract and merge ECMA XML API documentation via JSON.

Subcommands:
  extract   Extract "To be added." placeholder entries from XML to JSON
  merge     Merge agent-filled JSON back into XML (CDATA-safe via lxml)

Usage:
  python3 docs-tool.py extract docs/SkiaSharpAPI/ -o work/
  python3 docs-tool.py merge work/ --validate
"""

import argparse
import json
import os
import sys
from pathlib import Path

try:
    from lxml import etree
except ImportError:
    print("Error: lxml is required. Install with: pip install lxml", file=sys.stderr)
    sys.exit(1)


# ---------------------------------------------------------------------------
# Shared helpers
# ---------------------------------------------------------------------------

def parse_xml(xml_path):
    """Parse an ECMA XML file preserving CDATA."""
    parser = etree.XMLParser(strip_cdata=False, remove_blank_text=False)
    try:
        return etree.parse(xml_path, parser)
    except etree.XMLSyntaxError as e:
        print(f"  Warning: Skipping {xml_path}: {e}", file=sys.stderr)
        return None


def find_docs_by_docid(root, doc_id):
    """Find a <Docs> element by the parent Member's DocId signature."""
    for member in root.iter("Member"):
        for sig in member.findall("MemberSignature"):
            if sig.get("Language") == "DocId" and sig.get("Value") == doc_id:
                return member.find("Docs")
    return None


def get_element_text(elem):
    """Get the full text content of an element including children."""
    return (elem.text or "") + "".join(
        etree.tostring(child, encoding="unicode") for child in elem
    )


def set_element_content(elem, content):
    """Set element content, parsing XML fragments like <see cref="..." />.

    Handles mixed content: plain text + inline XML elements.
    If content is empty/None, makes element self-closing.
    """
    if not content or content.strip() == "":
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
        elem.text = None
        for child in list(elem):
            elem.remove(child)
        elem.text = fragment.text
        for child in fragment:
            elem.append(child)
    except etree.XMLSyntaxError:
        # Not valid XML — treat as plain text
        for child in list(elem):
            elem.remove(child)
        elem.text = content


def count_signatures(root):
    """Count total MemberSignature + TypeSignature elements."""
    return len(root.findall(".//MemberSignature")) + len(root.findall(".//TypeSignature"))


# ---------------------------------------------------------------------------
# Extract
# ---------------------------------------------------------------------------

def extract_docs_block(docs_elem):
    """Extract fields with 'To be added.' from a <Docs> block."""
    fields = {}
    for child in docs_elem:
        tag = child.tag
        text = get_element_text(child)
        if tag == "param":
            name = child.get("name", "?")
            if "To be added" in (text or ""):
                fields.setdefault("params", {})[name] = text
        elif tag == "typeparam":
            name = child.get("name", "?")
            if "To be added" in (text or ""):
                fields.setdefault("typeparams", {})[name] = text
        elif tag in ("summary", "returns", "value", "remarks"):
            if text and "To be added" in text:
                fields[tag] = text
    return fields if fields else None


def extract_file(xml_path):
    """Extract placeholder docs entries from an ECMA XML file."""
    tree = parse_xml(xml_path)
    if tree is None:
        return None

    root = tree.getroot()
    type_name = root.get("FullName", root.get("Name", "Unknown"))
    entries = []

    # Type-level docs
    type_docs = root.find("Docs")
    if type_docs is not None:
        fields = extract_docs_block(type_docs)
        if fields:
            entries.append({"docId": None, "memberType": "type", "fields": fields})

    # Member-level docs
    for member in root.iter("Member"):
        doc_id = cs_sig = member_type = None
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
            fields = extract_docs_block(docs)
            if fields:
                entries.append({
                    "docId": doc_id,
                    "memberType": member_type,
                    "memberName": member.get("MemberName", "?"),
                    "signature": cs_sig,
                    "fields": fields,
                })

    if not entries:
        return None
    return {"file": xml_path, "typeName": type_name, "entries": entries}


def cmd_extract(args):
    """Extract command."""
    os.makedirs(args.output, exist_ok=True)
    path = Path(args.path)
    xml_files = [path] if path.is_file() else sorted(path.rglob("*.xml"))

    total_entries = total_files = 0
    for xml_file in xml_files:
        result = extract_file(str(xml_file))
        if result and result["entries"]:
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


# ---------------------------------------------------------------------------
# Merge
# ---------------------------------------------------------------------------

def merge_file(json_path, dry_run=False, validate=False):
    """Merge a single JSON file back into its source XML. Returns field count."""
    with open(json_path) as f:
        data = json.load(f)

    xml_path = data["file"]
    if not os.path.exists(xml_path):
        print(f"  Warning: XML file not found: {xml_path}", file=sys.stderr)
        return 0

    tree = parse_xml(xml_path)
    if tree is None:
        return 0
    root = tree.getroot()
    sig_count_before = count_signatures(root)
    updates = 0

    for entry in data.get("entries", []):
        doc_id = entry.get("docId")
        member_type = entry.get("memberType")
        fields = entry.get("fields", {})

        # Find target Docs block
        if member_type == "type" or doc_id is None:
            docs = root.find("Docs")
        else:
            docs = find_docs_by_docid(root, doc_id)
        if docs is None:
            if doc_id:
                print(f"  Warning: DocId not found: {doc_id}", file=sys.stderr)
            continue

        # Update scalar fields
        for field_name in ("summary", "returns", "value", "remarks"):
            if field_name in fields:
                content = fields[field_name]
                if content is not None and "To be added" not in content:
                    elem = docs.find(field_name)
                    if elem is not None:
                        if dry_run:
                            print(f"  Would update {doc_id or 'type'}.{field_name}")
                        else:
                            set_element_content(elem, content)
                        updates += 1

        # Update params
        for param_map_key in ("params", "typeparams"):
            xml_tag = "param" if param_map_key == "params" else "typeparam"
            if param_map_key in fields:
                for name, content in fields[param_map_key].items():
                    if content is not None and "To be added" not in content:
                        for elem in docs.findall(xml_tag):
                            if elem.get("name") == name:
                                if dry_run:
                                    print(f"  Would update {doc_id or 'type'}.{xml_tag}.{name}")
                                else:
                                    set_element_content(elem, content)
                                updates += 1
                                break

    if updates == 0:
        return 0

    # Safety assertion
    sig_count_after = count_signatures(root)
    if sig_count_after != sig_count_before:
        print(f"  FATAL: Signature count changed in {xml_path} "
              f"({sig_count_before} -> {sig_count_after}). Aborting.", file=sys.stderr)
        sys.exit(2)

    if not dry_run:
        tree.write(xml_path, encoding="utf-8", xml_declaration=False, pretty_print=False)
        if validate:
            try:
                etree.parse(xml_path, etree.XMLParser())
            except etree.XMLSyntaxError as e:
                print(f"  ERROR: Malformed output: {xml_path}: {e}", file=sys.stderr)
                return -1

    return updates


def cmd_merge(args):
    """Merge command."""
    path = Path(args.path)
    json_files = [path] if path.is_file() else sorted(path.glob("*.json"))

    total_updates = total_files = 0
    for json_file in json_files:
        updates = merge_file(str(json_file), dry_run=args.dry_run, validate=args.validate)
        if updates > 0:
            total_files += 1
            total_updates += updates
            print(f"  {json_file.name}: {updates} fields")
        elif updates < 0:
            print(f"  {json_file.name}: ERROR")

    action = "Would update" if args.dry_run else "Merged"
    print(f"\n{action} {total_updates} fields across {total_files} files")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="ECMA XML documentation tool")
    sub = parser.add_subparsers(dest="command", required=True)

    ext = sub.add_parser("extract", help="Extract placeholders to JSON")
    ext.add_argument("path", help="XML file or directory")
    ext.add_argument("--output", "-o", required=True, help="Output directory")

    mrg = sub.add_parser("merge", help="Merge filled JSON back into XML")
    mrg.add_argument("path", help="JSON file or directory")
    mrg.add_argument("--dry-run", action="store_true")
    mrg.add_argument("--validate", action="store_true")

    args = parser.parse_args()
    if args.command == "extract":
        cmd_extract(args)
    elif args.command == "merge":
        cmd_merge(args)


if __name__ == "__main__":
    main()
