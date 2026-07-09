# Worked example

One real release-notes page end to end, so you can see exactly what each stage of
the pipeline produces. The version is **4.148.0** — the first stable v4 release, a
deliberately rich case (it rolls up several previews, has real breaking changes,
and credits multiple community contributors).

| File | Stage | Who writes it |
|---|---|---|
| `4.148.0.data.json` | facts | `build-data.py` — PRs (each tagged + community flag), the contributor roster, preview buckets, breaking-change *sources*, banner + link facts. Machine-owned; the agent never edits it. |
| `4.148.0.prose.json` | prose | the Polish agent — only `theme`, `highlights_*`, `breaking`, `categories`, `contributor_summaries`, `preview_summaries`. No headings, tables, handles, ❤️, or links. |
| `4.148.0.rendered.md` | page | `render-notes.py 4.148.0.data.json 4.148.0.prose.json 4.148.0.rendered.md` — every heading, table, banner, `@handle`, ❤️, and PR link. |

The page is exactly `render(data.json, prose.json)`, so you can reproduce it:

```sh
python3 ../scripts/render-notes.py 4.148.0.data.json 4.148.0.prose.json /tmp/out.md
diff 4.148.0.rendered.md /tmp/out.md   # identical
```

Study `4.148.0.prose.json` next to the rendered page to see how little prose the
agent supplies and how the renderer turns it into the finished page. See
`../SKILL.md` for the field-by-field guide.
