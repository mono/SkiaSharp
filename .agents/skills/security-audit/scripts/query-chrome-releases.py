#!/usr/bin/env python3
"""
Query the Chrome Releases blog for security-relevant posts mentioning Skia, ANGLE,
or other SkiaSharp-relevant components.

Two-pass extraction:
  Pass 1 (deterministic): Regex extracts structured CVE entries from the known blog format.
  Pass 2 (AI review):     Full post text is included for AI to find anything regex missed.

Prerequisites:
  - Python 3.8+ (uses urllib, no external dependencies)

Usage:
  # Standard: query last 6 months, write JSON cache
  python3 query-chrome-releases.py --output output/ai/chrome-releases-cache.json

  # Query last 12 months
  python3 query-chrome-releases.py --months 12 --output output/ai/chrome-releases-cache.json

  # Force re-fetch even if cache is fresh
  python3 query-chrome-releases.py --force --output output/ai/chrome-releases-cache.json

  # Custom keywords
  python3 query-chrome-releases.py --keywords CVE,Skia,ANGLE --output output/ai/chrome-releases-cache.json

Output:
  JSON written to --output file with:
    - structured_cves[]: deterministically extracted CVEs (high confidence)
    - posts[]: full text content of matching posts (for AI review)
  Progress messages print to stdout.
"""

import argparse
import html
import json
import os
import re
import sys
import time
import urllib.request
import urllib.error
from datetime import datetime, timedelta, timezone
from html.parser import HTMLParser
from xml.etree import ElementTree


# --- Constants ---

FEED_URL = "https://chromereleases.googleblog.com/feeds/posts/default"
ATOM_NS = "{http://www.w3.org/2005/Atom}"

DEFAULT_KEYWORDS = [
    "CVE", "security", "Skia", "ANGLE", "Canvas", "GPU", "Fonts",
    "vulnerability", "heap buffer", "out of bounds", "use after free",
    "integer overflow", "type confusion",
]

# Components that are relevant to SkiaSharp
RELEVANT_COMPONENTS = [
    "Skia", "ANGLE", "Canvas", "Fonts", "GPU", "Graphics",
    "Compositing", "WebGL", "Vulkan",
]

# Regex for the known Chrome Releases CVE format:
# [$bounty][bug_id] Severity CVE-YYYY-NNNN: Description. Reported by X on YYYY-MM-DD
CVE_ENTRY_REGEX = re.compile(
    r"\[([^\]]*)\]\s*\[(\d+)\]\s*"          # [$bounty][bug_id]
    r"(Critical|High|Medium|Low)\s+"          # Severity
    r"(CVE-\d{4}-\d{4,})\s*:\s*"             # CVE ID
    r"(.+?)\.\s*"                             # Description
    r"(?:Reported\s+by\s+(.+?)\s+on\s+"      # Reporter (optional)
    r"(\d{4}-\d{2}-\d{2}))?",                # Date (optional)
    re.IGNORECASE
)

# Regex to extract Chrome version from post content
CHROME_VERSION_REGEX = re.compile(
    r"(\d{2,3}\.\d+\.\d+\.\d+)"
)

CACHE_MAX_AGE_HOURS = 24


# --- HTML stripping ---

class HTMLTextExtractor(HTMLParser):
    """Strip HTML tags, decode entities, produce plain text."""

    def __init__(self):
        super().__init__()
        self._text_parts = []
        self._skip = False

    def handle_starttag(self, tag, attrs):
        if tag in ("script", "style", "template"):
            self._skip = True

    def handle_endtag(self, tag):
        if tag in ("script", "style", "template"):
            self._skip = False
        if tag in ("p", "br", "div", "li", "tr"):
            self._text_parts.append("\n")

    def handle_data(self, data):
        if not self._skip:
            self._text_parts.append(data)

    def handle_entityref(self, name):
        char = html.unescape(f"&{name};")
        self._text_parts.append(char)

    def handle_charref(self, name):
        char = html.unescape(f"&#{name};")
        self._text_parts.append(char)

    def get_text(self):
        return "".join(self._text_parts).strip()


def strip_html(html_content: str) -> str:
    """Convert HTML to plain text."""
    extractor = HTMLTextExtractor()
    try:
        extractor.feed(html_content)
    except Exception:
        # Fallback: crude regex strip
        text = re.sub(r"<[^>]+>", " ", html_content)
        return html.unescape(text).strip()
    return extractor.get_text()


# --- Feed fetching ---

def fetch_feed_page(url: str, retries: int = 3) -> ElementTree.Element:
    """Fetch a single Atom feed page and return the parsed XML root."""
    for attempt in range(retries):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "SkiaSharp-SecurityAudit/1.0"})
            with urllib.request.urlopen(req, timeout=30) as resp:
                data = resp.read()
                return ElementTree.fromstring(data)
        except (urllib.error.URLError, urllib.error.HTTPError, TimeoutError) as e:
            if attempt < retries - 1:
                wait = 2 ** attempt
                print(f"  ⚠️  Fetch failed ({e}), retrying in {wait}s...")
                time.sleep(wait)
            else:
                raise


def fetch_all_posts(months: int) -> list:
    """Fetch blog posts from the last N months via Atom feed pagination."""
    cutoff = datetime.now(timezone.utc) - timedelta(days=months * 30)
    posts = []
    url = FEED_URL
    page = 0

    while url:
        page += 1
        print(f"  Fetching feed page {page}...")
        root = fetch_feed_page(url)

        entries = root.findall(f"{ATOM_NS}entry")
        if not entries:
            break

        reached_cutoff = False
        for entry in entries:
            published_el = entry.find(f"{ATOM_NS}published")
            if published_el is None:
                continue

            # Parse date (format: 2026-05-28T20:00:00.000-07:00)
            pub_str = published_el.text.strip()
            try:
                # Handle various timezone formats
                pub_date = datetime.fromisoformat(pub_str.replace("Z", "+00:00"))
            except ValueError:
                # Fallback: try without timezone
                try:
                    pub_date = datetime.fromisoformat(pub_str[:19]).replace(tzinfo=timezone.utc)
                except ValueError:
                    continue

            if pub_date < cutoff:
                reached_cutoff = True
                break

            # Extract post data
            title_el = entry.find(f"{ATOM_NS}title")
            content_el = entry.find(f"{ATOM_NS}content")
            link_el = entry.find(f"{ATOM_NS}link[@rel='alternate']")
            categories = entry.findall(f"{ATOM_NS}category")

            title = title_el.text if title_el is not None and title_el.text else ""
            content_html = content_el.text if content_el is not None and content_el.text else ""
            link = link_el.get("href", "") if link_el is not None else ""
            labels = [c.get("term", "") for c in categories if c.get("term")]

            posts.append({
                "title": title,
                "url": link,
                "published": pub_date.strftime("%Y-%m-%d"),
                "labels": labels,
                "content_html": content_html,
            })

        if reached_cutoff:
            break

        # Find next page link
        next_link = root.find(f"{ATOM_NS}link[@rel='next']")
        if next_link is not None:
            url = next_link.get("href", "")
            if not url:
                break
            # Brief pause to be polite
            time.sleep(0.5)
        else:
            break

    print(f"  Fetched {len(posts)} posts from {page} page(s)")
    return posts


# --- Extraction ---

def matches_keywords(text: str, keywords: list) -> list:
    """Return list of keywords found in text (case-insensitive)."""
    text_lower = text.lower()
    return [kw for kw in keywords if kw.lower() in text_lower]


def extract_chrome_version(text: str) -> tuple:
    """Extract Chrome version and milestone from post text."""
    # Look for version string near the top of the post
    match = CHROME_VERSION_REGEX.search(text[:500])
    if match:
        version = match.group(1)
        milestone = int(version.split(".")[0])
        return version, milestone
    return None, None


def is_relevant_component(description: str) -> bool:
    """Check if a CVE description mentions a relevant component."""
    desc_lower = description.lower()
    return any(comp.lower() in desc_lower for comp in RELEVANT_COMPONENTS)


def extract_structured_cves(text: str, post_url: str, chrome_version: str, milestone: int) -> list:
    """Apply regex to extract structured CVE entries from post text."""
    cves = []
    for match in CVE_ENTRY_REGEX.finditer(text):
        bounty = match.group(1).strip()
        bug_id = match.group(2).strip()
        severity = match.group(3).strip()
        cve_id = match.group(4).strip()
        description = match.group(5).strip()
        reporter = (match.group(6) or "").strip() or None
        date_reported = (match.group(7) or "").strip() or None

        # Determine component from description
        component = "Unknown"
        for comp in RELEVANT_COMPONENTS:
            if comp.lower() in description.lower():
                component = comp
                break

        cves.append({
            "cve_id": cve_id,
            "severity": severity.capitalize(),
            "component": component,
            "description": description,
            "bug_id": bug_id,
            "bug_url": f"https://issues.chromium.org/issues/{bug_id}",
            "chrome_version": chrome_version,
            "milestone": milestone,
            "bounty": bounty if bounty != "N/A" else None,
            "reporter": reporter,
            "date_reported": date_reported,
            "blog_post_url": post_url,
            "extraction": "regex",
        })

    return cves


# --- Main ---

def process_posts(posts: list, keywords: list) -> dict:
    """Process fetched posts: filter by keywords, extract CVEs, build output."""
    all_structured_cves = []
    matching_posts = []
    skia_relevant_cves = []

    for post in posts:
        text = strip_html(post["content_html"])
        matched = matches_keywords(text, keywords)

        if not matched:
            continue

        chrome_version, milestone = extract_chrome_version(text)

        # Pass 1: Regex extraction
        structured = extract_structured_cves(
            text, post["url"], chrome_version, milestone
        )
        all_structured_cves.extend(structured)

        # Filter for Skia-relevant CVEs
        relevant = [c for c in structured if is_relevant_component(c["description"])]
        skia_relevant_cves.extend(relevant)

        matching_posts.append({
            "title": post["title"],
            "url": post["url"],
            "published": post["published"],
            "labels": post["labels"],
            "chrome_version": chrome_version,
            "milestone": milestone,
            "text_content": text,
            "matched_keywords": matched,
            "regex_cve_count": len(structured),
            "regex_relevant_cve_count": len(relevant),
        })

    return {
        "fetched_at": datetime.now(timezone.utc).isoformat(),
        "keywords_used": keywords,
        "total_posts_scanned": len(posts),
        "matching_posts_count": len(matching_posts),
        "total_regex_cves": len(all_structured_cves),
        "skia_relevant_regex_cves": len(skia_relevant_cves),
        "structured_cves": skia_relevant_cves,
        "all_structured_cves": all_structured_cves,
        "posts": matching_posts,
    }


def main():
    parser = argparse.ArgumentParser(
        description="Query Chrome Releases blog for Skia/ANGLE security CVEs"
    )
    parser.add_argument(
        "--output", required=True,
        help="Output JSON file path"
    )
    parser.add_argument(
        "--months", type=int, default=6,
        help="How many months back to query (default: 6)"
    )
    parser.add_argument(
        "--keywords", type=str, default=None,
        help="Comma-separated keywords to filter posts (default: built-in list)"
    )
    parser.add_argument(
        "--force", action="store_true",
        help="Force re-fetch even if cache is fresh"
    )
    parser.add_argument(
        "--verbose", action="store_true",
        help="Print extra progress information"
    )
    args = parser.parse_args()

    # Check cache freshness
    if not args.force and os.path.exists(args.output):
        mtime = os.path.getmtime(args.output)
        age_hours = (time.time() - mtime) / 3600
        if age_hours < CACHE_MAX_AGE_HOURS:
            # Validate cached params match requested params
            with open(args.output) as f:
                data = json.load(f)
            cached_months = data.get("months_queried", 0)
            if cached_months >= args.months:
                print(f"✅ Cache is fresh ({age_hours:.1f}h old, max {CACHE_MAX_AGE_HOURS}h). Use --force to re-fetch.")
                print(f"   {data.get('skia_relevant_regex_cves', 0)} Skia-relevant CVEs from {data.get('matching_posts_count', 0)} posts")
                sys.exit(0)
            else:
                print(f"⚠️  Cache covers {cached_months} months but {args.months} requested. Re-fetching...")

    # Parse keywords
    keywords = DEFAULT_KEYWORDS
    if args.keywords:
        keywords = [k.strip() for k in args.keywords.split(",") if k.strip()]

    print(f"🔍 Querying Chrome Releases blog (last {args.months} months)...")
    print(f"   Keywords: {', '.join(keywords[:8])}{'...' if len(keywords) > 8 else ''}")

    # Fetch posts
    posts = fetch_all_posts(args.months)

    if not posts:
        print("⚠️  No posts fetched. Check network connectivity.")
        sys.exit(1)

    # Process
    print(f"🔬 Processing {len(posts)} posts...")
    result = process_posts(posts, keywords)
    result["months_queried"] = args.months

    # Write output
    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    with open(args.output, "w") as f:
        json.dump(result, f, indent=2)

    # Summary
    print(f"\n✅ Chrome Releases query complete:")
    print(f"   Posts scanned: {result['total_posts_scanned']}")
    print(f"   Posts matching keywords: {result['matching_posts_count']}")
    print(f"   Total CVEs extracted (regex): {result['total_regex_cves']}")
    print(f"   Skia-relevant CVEs (regex): {result['skia_relevant_regex_cves']}")
    print(f"   Output: {args.output}")

    if args.verbose and result["structured_cves"]:
        print(f"\n   Skia-relevant CVEs found:")
        for cve in result["structured_cves"]:
            print(f"     • {cve['cve_id']} ({cve['severity']}) — {cve['description']} [m{cve['milestone']}]")

    print(f"\n   ℹ️  AI should also review the {result['matching_posts_count']} post(s) text_content")
    print(f"      for any CVEs the regex didn't catch (format variations, indirect mentions).")


if __name__ == "__main__":
    main()
