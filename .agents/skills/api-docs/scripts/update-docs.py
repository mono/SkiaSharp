#!/usr/bin/env python3
"""
update-docs.py — Safely update <Docs> blocks in ECMA XML API documentation files.

Only modifies content inside <Docs> elements. Structurally cannot touch
<MemberSignature>, <TypeSignature>, or any other non-docs elements.

Usage:
  # Update a member's docs (identified by DocId)
  python3 update-docs.py SKCanvas.xml \
    --member "M:SkiaSharp.SKCanvas.DrawRect(SkiaSharp.SKRect,SkiaSharp.SKPaint)" \
    --summary "Draws a rectangle using the specified paint." \
    --param "rect=The rectangle to draw." \
    --param "paint=The paint to use for drawing." \
    --remarks ""

  # Update type-level docs
  python3 update-docs.py SKCanvas.xml \
    --type \
    --summary "Encapsulates all drawing state."

  # List members with "To be added." placeholders
  python3 update-docs.py SKCanvas.xml --list-placeholders
"""

import argparse
import sys
import xml.etree.ElementTree as ET


def indent_xml(elem, level=0, indent="  "):
    """Re-indent an element tree for consistent formatting."""
    i = "\n" + level * indent
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + indent
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
        for child in elem:
            indent_xml(child, level + 1, indent)
        if not child.tail or not child.tail.strip():
            child.tail = i
    else:
        if not elem.tail or not elem.tail.strip():
            elem.tail = i


def find_member_by_docid(root, docid):
    """Find a <Member> element by its DocId MemberSignature."""
    for member in root.iter("Member"):
        for sig in member.findall("MemberSignature"):
            if sig.get("Language") == "DocId" and sig.get("Value") == docid:
                return member
    return None


def update_docs_block(docs_elem, summary=None, value=None, returns=None,
                      remarks=None, params=None):
    """Update children of a <Docs> element. Only touches specified fields."""
    if summary is not None:
        elem = docs_elem.find("summary")
        if elem is not None:
            elem.text = summary
            # Remove any children (like <see> refs) if we're replacing entirely
            for child in list(elem):
                elem.remove(child)

    if value is not None:
        elem = docs_elem.find("value")
        if elem is not None:
            elem.text = value if value else None
            for child in list(elem):
                elem.remove(child)

    if returns is not None:
        elem = docs_elem.find("returns")
        if elem is not None:
            elem.text = returns if returns else None
            for child in list(elem):
                elem.remove(child)

    if remarks is not None:
        elem = docs_elem.find("remarks")
        if elem is not None:
            if remarks == "" or remarks is None:
                elem.text = None
                for child in list(elem):
                    elem.remove(child)
            else:
                elem.text = remarks

    if params:
        for name, text in params.items():
            for param in docs_elem.findall("param"):
                if param.get("name") == name:
                    param.text = text
                    for child in list(param):
                        param.remove(child)
                    break
            else:
                print(f"  Warning: param '{name}' not found in Docs block",
                      file=sys.stderr)


def list_placeholders(root, filepath):
    """List all members with 'To be added.' placeholders."""
    # Type-level docs
    type_docs = root.find("Docs")
    if type_docs is not None:
        for child in type_docs:
            if child.text and "To be added" in child.text:
                type_name = root.get("FullName", root.get("Name", "?"))
                print(f"  --type  ({child.tag}: To be added.)")
                break

    # Member-level docs
    for member in root.iter("Member"):
        docs = member.find("Docs")
        if docs is None:
            continue
        has_placeholder = False
        for child in docs:
            if child.text and "To be added" in child.text:
                has_placeholder = True
                break
        if has_placeholder:
            docid_sig = None
            for sig in member.findall("MemberSignature"):
                if sig.get("Language") == "DocId":
                    docid_sig = sig.get("Value")
                    break
            name = member.get("MemberName", "?")
            tags = []
            for child in docs:
                if child.text and "To be added" in child.text:
                    tags.append(child.tag)
            print(f"  --member \"{docid_sig}\"  "
                  f"({', '.join(tags)} need docs)")


def main():
    parser = argparse.ArgumentParser(
        description="Safely update <Docs> blocks in ECMA XML files.")
    parser.add_argument("file", help="Path to the XML file")
    parser.add_argument("--member", help="DocId of the member to update")
    parser.add_argument("--type", action="store_true",
                        help="Update type-level docs instead of a member")
    parser.add_argument("--summary", help="Summary text")
    parser.add_argument("--value", help="Value text (properties)")
    parser.add_argument("--returns", help="Returns text (methods)")
    parser.add_argument("--remarks", help="Remarks text (empty string = self-closing)")
    parser.add_argument("--param", action="append", default=[],
                        help="Parameter docs as name=text (repeatable)")
    parser.add_argument("--list-placeholders", action="store_true",
                        help="List members with 'To be added.' placeholders")
    parser.add_argument("--dry-run", action="store_true",
                        help="Print changes without writing")

    args = parser.parse_args()

    # Parse the XML file, preserving the original structure
    try:
        tree = ET.parse(args.file)
    except ET.ParseError as e:
        print(f"Error: Failed to parse {args.file}: {e}", file=sys.stderr)
        sys.exit(1)

    root = tree.getroot()

    if args.list_placeholders:
        print(f"{args.file}:")
        list_placeholders(root, args.file)
        return

    # Parse params
    params = {}
    for p in args.param:
        if "=" not in p:
            print(f"Error: --param must be name=text, got: {p}", file=sys.stderr)
            sys.exit(1)
        name, text = p.split("=", 1)
        params[name] = text

    # Find the target Docs block
    if args.type:
        docs = root.find("Docs")
        if docs is None:
            print(f"Error: No type-level <Docs> in {args.file}", file=sys.stderr)
            sys.exit(1)
        target_desc = f"type {root.get('FullName', root.get('Name', '?'))}"
    elif args.member:
        member = find_member_by_docid(root, args.member)
        if member is None:
            print(f"Error: No member with DocId '{args.member}' in {args.file}",
                  file=sys.stderr)
            sys.exit(1)
        docs = member.find("Docs")
        if docs is None:
            print(f"Error: Member '{args.member}' has no <Docs> block",
                  file=sys.stderr)
            sys.exit(1)
        target_desc = args.member
    else:
        print("Error: Must specify --member or --type", file=sys.stderr)
        sys.exit(1)

    # Count signatures before (safety check)
    sig_count_before = len(list(root.iter("MemberSignature")))
    type_sig_count_before = len(list(root.iter("TypeSignature")))

    # Apply updates
    update_docs_block(docs, summary=args.summary, value=args.value,
                      returns=args.returns, remarks=args.remarks,
                      params=params if params else None)

    # Safety check: signature count must not change
    sig_count_after = len(list(root.iter("MemberSignature")))
    type_sig_count_after = len(list(root.iter("TypeSignature")))
    if sig_count_after != sig_count_before:
        print(f"FATAL: MemberSignature count changed "
              f"({sig_count_before} → {sig_count_after}). Bug in script!",
              file=sys.stderr)
        sys.exit(2)
    if type_sig_count_after != type_sig_count_before:
        print(f"FATAL: TypeSignature count changed. Bug in script!",
              file=sys.stderr)
        sys.exit(2)

    if args.dry_run:
        print(f"Would update {target_desc}:")
        if args.summary:
            print(f"  summary: {args.summary}")
        if args.value:
            print(f"  value: {args.value}")
        if args.returns:
            print(f"  returns: {args.returns}")
        if args.remarks is not None:
            print(f"  remarks: {args.remarks or '(empty)'}")
        for name, text in params.items():
            print(f"  param {name}: {text}")
        return

    # Write back
    tree.write(args.file, encoding="unicode", xml_declaration=False)
    print(f"Updated {target_desc}")


if __name__ == "__main__":
    main()
