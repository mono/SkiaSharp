# Worked examples

Two real release-notes pages end to end, so you can see exactly what each stage of
the pipeline produces — and how the same machinery scales from a light release to a
heavy one.

- **`4.148.0`** — the first stable v4 release: a deliberately *rich* case that rolls
  up several previews, has real **breaking changes**, and credits multiple community
  contributors (1423-word page).
- **`4.151.0`** — a *light* preview: 15 PRs, **no breaking changes** (so the page
  shows the `*None in this preview line.*` fallback), a couple of category bullets,
  and one contributor (155-word page).

Each example is three files:

| File | Stage | Who writes it |
|---|---|---|
| `<v>.data.json` | facts | `build-data.py` — PRs (each tagged + community flag), the contributor roster, preview buckets, breaking-change *sources*, banner + link facts. Machine-owned; the agent never edits it. |
| `<v>.prose.json` | prose | the Polish agent — only `theme`, `highlights_*`, `breaking`, `categories`, `contributor_summaries`, `preview_summaries`. No headings, tables, handles, ❤️, or links. |
| `<v>.rendered.md` | page | `render-notes.py <v>.data.json <v>.prose.json <v>.rendered.md` — every heading, table, banner, `@handle`, ❤️, and PR link. |

The page is exactly `render(data.json, prose.json)`, so you can reproduce either:

```sh
python3 ../scripts/render-notes.py 4.148.0.data.json 4.148.0.prose.json /tmp/out.md
diff 4.148.0.rendered.md /tmp/out.md   # identical
```

Read a `<v>.prose.json` next to its rendered page to see how little prose the agent
supplies and how the renderer turns it into the finished page. See `../SKILL.md`
for the field-by-field guide.
