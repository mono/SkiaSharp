#!/usr/bin/env python3
"""Convert a cached issue JSON file into annotated markdown for AI analysis.

Usage:
    issue-to-markdown.py <path-to-cached-item.json>
    issue-to-markdown.py .data-cache/repos/mono-SkiaSharp/github/items/2794.json

Can also read from stdin (e.g., piped from gh CLI):
    gh api repos/mono/SkiaSharp/issues/2794 | issue-to-markdown.py -

Output: annotated markdown to stdout.

Annotations:
  - [OP] / [MEMBER] / [CONTRIBUTOR] / [BOT] author tags
  - Time-deltas between comments
  - Attachments & Links summary table
  - Bot comments collapsed to single lines
  - All image/zip/repo URLs preserved
"""

import json
import re
import sys
from datetime import datetime, timezone
from pathlib import Path


def parse_date(s: str | None) -> datetime | None:
    if not s:
        return None
    try:
        # Handle both "2024-03-12T10:17:05Z" and "2024-03-12T10:17:05+00:00"
        return datetime.fromisoformat(s.replace("Z", "+00:00"))
    except (ValueError, TypeError):
        return None


def format_delta(d1: datetime, d2: datetime) -> str:
    """Human-readable delta between two datetimes."""
    delta = d2 - d1
    days = delta.days
    if days == 0:
        return "+0d"
    elif days < 30:
        return f"+{days}d"
    elif days < 365:
        months = days // 30
        return f"+{months}mo"
    else:
        years = days // 365
        return f"+{years}y"


def classify_author(login: str, issue_author: str, author_type: str | None = None,
                    author_association: str | None = None) -> str:
    """Determine author role tag."""
    if not login:
        return ""
    if login.endswith("[bot]"):
        return "[BOT]"
    if login == issue_author:
        return "[OP]"
    if author_association in ("MEMBER", "OWNER"):
        return "[MEMBER]"
    if author_type and author_type.lower() in ("microsoft", "member"):
        return "[MEMBER]"
    if author_association == "CONTRIBUTOR":
        return "[CONTRIBUTOR]"
    return ""


def extract_links(text: str) -> dict:
    """Extract all image URLs, zip files, repo links, and related issues from text."""
    result = {"images": [], "zips": [], "repos": [], "issues": []}

    # Images: <img src="..."> and ![...](...)
    for url in re.findall(r'<img[^>]+src="([^"]+)"', text):
        result["images"].append(url)
    for url in re.findall(r'!\[[^\]]*\]\(([^)]+)\)', text):
        result["images"].append(url)

    # Zip files: [name.zip](url)
    for match in re.findall(r'\[([^\]]*\.(?:zip|rar|7z|tar\.gz))\]\(([^)]+)\)', text, re.IGNORECASE):
        result["zips"].append({"filename": match[0], "url": match[1]})

    # GitHub repo links (not issue/PR links, not assets)
    for url in re.findall(r'https://github\.com/([^/\s]+/[^/\s]+?)(?:/tree/[^\s)]+|(?=[\s)\]]|$))', text):
        # Filter out mono/SkiaSharp self-references that are issue links
        if not re.match(r'mono/SkiaSharp$', url):
            repo_url = f"https://github.com/{url}"
            if repo_url not in [z["url"] for z in result["zips"]]:
                result["repos"].append(repo_url)

    # Related issues: #NNN or full GitHub issue URLs
    for num in re.findall(r'https://github\.com/mono/SkiaSharp/issues/(\d+)', text):
        result["issues"].append(int(num))
    # Shorthand #NNN (only at word boundary, not in URLs)
    for num in re.findall(r'(?<!\w)#(\d+)\b', text):
        n = int(num)
        if n not in result["issues"] and n > 0:
            result["issues"].append(n)

    return result


def format_reactions(reactions: dict | list | None) -> str:
    """Format reactions into emoji summary."""
    if not reactions:
        return ""

    if isinstance(reactions, dict):
        # GitHub API format: {"+1": 2, "-1": 0, "heart": 1, ...}
        emoji_map = {"+1": "👍", "-1": "👎", "laugh": "😄", "confused": "😕",
                     "heart": "❤️", "hooray": "🎉", "rocket": "🚀", "eyes": "👀"}
        parts = []
        total = reactions.get("total_count", 0)
        if total == 0:
            return ""
        for key, emoji in emoji_map.items():
            count = reactions.get(key, 0)
            if count > 0:
                parts.append(f"{emoji}{count}")
        return " ".join(parts)

    if isinstance(reactions, list):
        # Cache format: [{user, content, createdAt}, ...]
        from collections import Counter
        emoji_map = {"+1": "👍", "-1": "👎", "laugh": "😄", "confused": "😕",
                     "heart": "❤️", "hooray": "🎉", "rocket": "🚀", "eyes": "👀"}
        counts = Counter(r.get("content", "") for r in reactions)
        parts = []
        for key, emoji in emoji_map.items():
            if counts.get(key, 0) > 0:
                parts.append(f"{emoji}{counts[key]}")
        return " ".join(parts)

    return ""


def clean_body(text: str) -> str:
    """Clean up body text while preserving all URLs and code blocks."""
    if not text:
        return ""
    # Normalize line endings
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    # Collapse 3+ blank lines to 2
    text = re.sub(r'\n{3,}', '\n\n', text)
    # Strip trailing whitespace per line
    text = "\n".join(line.rstrip() for line in text.split("\n"))
    return text.strip()


def process_issue(data: dict) -> str:
    """Convert issue JSON to annotated markdown."""
    lines = []

    # Determine data format (GitHub API vs cached format)
    # GitHub API: user.login, author_association
    # Cache: author.login, author.type
    if "user" in data:
        # GitHub API format
        issue_author = data.get("user", {}).get("login", "unknown")
        author_assoc = data.get("author_association", "NONE")
    else:
        # Cache format
        issue_author = data.get("author", {}).get("login", "unknown") if isinstance(data.get("author"), dict) else str(data.get("author", "unknown"))
        author_assoc = None

    number = data.get("number", "?")
    title = data.get("title", "Untitled")
    state = data.get("state", "unknown")
    if isinstance(state, dict):
        state = state.get("stringValue", str(state))

    created = parse_date(data.get("created_at") or data.get("createdAt"))
    updated = parse_date(data.get("updated_at") or data.get("updatedAt"))
    closed = parse_date(data.get("closed_at") or data.get("closedAt"))

    # Labels
    labels_raw = data.get("labels", [])
    label_names = []
    for l in labels_raw:
        if isinstance(l, dict):
            label_names.append(l.get("name", str(l)))
        else:
            label_names.append(str(l))

    # Get comments
    comments = []
    # Cache format: engagement.comments
    engagement = data.get("engagement")
    if engagement and isinstance(engagement, dict):
        comments = engagement.get("comments", [])

    # GitHub API format: separate comments array (not in issue JSON)
    # These would be passed in via --comments flag or stdin

    # Count comment authors
    op_count = 0
    member_count = 0
    contributor_count = 0
    bot_count = 0
    community_count = 0

    for c in comments:
        login = ""
        assoc = None
        c_type = None
        if isinstance(c.get("user"), dict):
            login = c["user"].get("login", "")
            assoc = c.get("author_association")
        elif isinstance(c.get("author"), str):
            login = c["author"]
        elif isinstance(c.get("author"), dict):
            login = c["author"].get("login", "")
            c_type = c["author"].get("type")

        role = classify_author(login, issue_author, c_type, assoc)
        if role == "[BOT]":
            bot_count += 1
        elif role == "[OP]":
            op_count += 1
        elif role == "[MEMBER]":
            member_count += 1
        elif role == "[CONTRIBUTOR]":
            contributor_count += 1
        else:
            community_count += 1

    total_comments = data.get("comments", data.get("commentCount", len(comments)))
    comment_dist_parts = []
    if op_count:
        comment_dist_parts.append(f"{op_count} OP")
    if member_count:
        comment_dist_parts.append(f"{member_count} member")
    if contributor_count:
        comment_dist_parts.append(f"{contributor_count} contributor")
    if community_count:
        comment_dist_parts.append(f"{community_count} community")
    if bot_count:
        comment_dist_parts.append(f"{bot_count} bot")
    comment_summary = f"{total_comments} ({', '.join(comment_dist_parts)})" if comment_dist_parts else str(total_comments)

    # Reactions
    reactions_raw = data.get("reactions")
    if engagement:
        reactions_raw = engagement.get("reactions", reactions_raw)
    reaction_str = format_reactions(reactions_raw)
    reaction_total = 0
    if isinstance(reactions_raw, dict):
        reaction_total = reactions_raw.get("total_count", 0)
    elif isinstance(reactions_raw, list):
        reaction_total = len(reactions_raw)

    # Header
    lines.append(f"# Issue #{number}: {title}")
    lines.append("")
    lines.append("| Field | Value |")
    lines.append("|-------|-------|")
    lines.append(f"| State | {state} |")
    lines.append(f"| Author | {issue_author} |")
    if created:
        lines.append(f"| Created | {created.strftime('%Y-%m-%d')} |")
    if updated:
        lines.append(f"| Updated | {updated.strftime('%Y-%m-%d')} |")
    if closed:
        lines.append(f"| Closed | {closed.strftime('%Y-%m-%d')} |")
    if label_names:
        lines.append(f"| Labels | {', '.join(label_names)} |")
    if reaction_total > 0:
        lines.append(f"| Reactions | {reaction_str} ({reaction_total} total) |")
    lines.append(f"| Comments | {comment_summary} |")
    if data.get("milestone"):
        ms = data["milestone"]
        ms_name = ms.get("title", str(ms)) if isinstance(ms, dict) else str(ms)
        lines.append(f"| Milestone | {ms_name} |")

    # Collect ALL links from body + comments
    body = data.get("body", "")
    all_links = {"images": [], "zips": [], "repos": [], "issues": []}
    sources = []

    body_links = extract_links(body)
    for url in body_links["images"]:
        all_links["images"].append(url)
        sources.append(("📸 screenshot", "description [OP]", url))
    for z in body_links["zips"]:
        all_links["zips"].append(z)
        sources.append(("📎 " + z["filename"], "description [OP]", z["url"]))
    for url in body_links["repos"]:
        all_links["repos"].append(url)
        sources.append(("🔗 repo", "description [OP]", url))
    for num in body_links["issues"]:
        all_links["issues"].append(num)
        sources.append(("🔗 issue", "description [OP]", f"#{num}"))

    for i, c in enumerate(comments):
        c_body = c.get("body", "")
        login = ""
        assoc = None
        c_type = None
        if isinstance(c.get("user"), dict):
            login = c["user"].get("login", "")
            assoc = c.get("author_association")
        elif isinstance(c.get("author"), str):
            login = c["author"]
        elif isinstance(c.get("author"), dict):
            login = c["author"].get("login", "")
            c_type = c["author"].get("type")

        role = classify_author(login, issue_author, c_type, assoc)
        source_label = f"comment {i+1} {role} {login}".strip()

        c_links = extract_links(c_body)
        for url in c_links["images"]:
            all_links["images"].append(url)
            sources.append(("📸 screenshot", source_label, url))
        for z in c_links["zips"]:
            all_links["zips"].append(z)
            sources.append(("📎 " + z["filename"], source_label, z["url"]))
        for url in c_links["repos"]:
            all_links["repos"].append(url)
            sources.append(("🔗 repo", source_label, url))
        for num in c_links["issues"]:
            if num not in all_links["issues"]:
                all_links["issues"].append(num)
                sources.append(("🔗 issue", source_label, f"#{num}"))

    # Attachments & Links table
    if sources:
        lines.append("")
        lines.append("## Attachments & Links")
        lines.append("")
        lines.append("| Type | Source | URL |")
        lines.append("|------|--------|-----|")
        for type_label, source, url in sources:
            lines.append(f"| {type_label} | {source} | {url} |")

    # Description
    lines.append("")
    lines.append("## Description")
    lines.append("")
    role = classify_author(issue_author, issue_author, None, author_assoc)
    date_str = created.strftime('%Y-%m-%d') if created else "?"
    lines.append(f"{role} {issue_author} — {date_str}")
    lines.append("")
    lines.append(clean_body(body))

    # Comments
    if comments:
        lines.append("")
        lines.append("## Comments")

        prev_date = created
        for i, c in enumerate(comments):
            login = ""
            assoc = None
            c_type = None
            if isinstance(c.get("user"), dict):
                login = c["user"].get("login", "")
                assoc = c.get("author_association")
            elif isinstance(c.get("author"), str):
                login = c["author"]
            elif isinstance(c.get("author"), dict):
                login = c["author"].get("login", "")
                c_type = c["author"].get("type")

            role = classify_author(login, issue_author, c_type, assoc)
            c_date = parse_date(c.get("created_at") or c.get("createdAt"))
            date_str = c_date.strftime('%Y-%m-%d') if c_date else "?"

            delta_str = ""
            if c_date and prev_date:
                delta_str = f" ({format_delta(prev_date, c_date)})"

            c_body = c.get("body", "")
            c_reactions = format_reactions(c.get("reactions"))
            rxn_str = f" {c_reactions}" if c_reactions else ""

            # Collapse bot comments
            if role == "[BOT]":
                first_line = c_body.split("\n")[0][:100]
                lines.append("")
                lines.append(f"> 🤖 {login}: {first_line}...")
            else:
                lines.append("")
                lines.append(f"### Comment {i+1} — {role} {login} — {date_str}{delta_str}{rxn_str}")
                lines.append("")
                lines.append(clean_body(c_body))

            if c_date:
                prev_date = c_date

    return "\n".join(lines)


def main():
    if len(sys.argv) < 2:
        print(f"Usage: {sys.argv[0]} <cached-item.json>", file=sys.stderr)
        print(f"       cat issue.json | {sys.argv[0]} -", file=sys.stderr)
        sys.exit(2)

    if sys.argv[1] == "-":
        data = json.load(sys.stdin)
    else:
        path = Path(sys.argv[1])
        if not path.exists():
            print(f"❌ File not found: {path}", file=sys.stderr)
            sys.exit(2)
        data = json.loads(path.read_text())

    print(process_issue(data))


if __name__ == "__main__":
    main()
