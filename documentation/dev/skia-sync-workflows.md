# Skia sync review workflow

A Skia sync is a reciprocal pair: a `[skia-sync]` `mono/SkiaSharp` parent pull
request and a `mono/skia` pull request on the same `skia-sync/*` branch. Both PR
bodies must contain one exact reciprocal HTTPS pull-request link. The resolver
requires an open same-repository pair, one `chrome/mNNN` milestone, and maps a
parent `main` base to native `skiasharp` (or the same `release/A.B.x` base).
`skia-sync/main` is deliberately rejected.

Comment `/skia-sync-review` on the parent PR to create a new, append-only,
exact-head review-evidence comment. The command runs the generator and
`git-sync-deps` in a tokenless Docker namespace, then a trusted host produces
the raw evidence. Terra writes the schema-v1 review report without a write
token or a comment target. The publisher requires successful threat detection,
derives every status, count, and risk from that raw evidence, scans the
canonical report for credential-like values, rechecks the complete live frozen
pair, uploads a distinct `skia-review-evidence` artifact, and only then posts
the marker, check-status table, risk, and immutable artifact link.

Manual dispatch requires the parent PR number and defaults to `staged: true`;
it validates and archives the report without posting a comment. Successful
reviews persist under the immutable
`ai-review/<native-pr>/<native-head>/<parent-head>/run-<run>-<attempt>/<report-digest>/`
path on `aw-data`; persistence rejects an existing file rather than overwriting
it. Review evidence is informative, not approval, and this workflow does not
repin, merge, create pull requests, or invoke release workflows.
