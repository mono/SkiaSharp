# Skia sync review and merge workflows

Skia milestone syncs are a reciprocal pair: a `[skia-sync]` `mono/SkiaSharp` PR and
a `mono/skia` PR on the identical `skia-sync/*` branch, targeting one
`chrome/mNNN` milestone. The repository resolver rejects ambiguous links, forks,
unsupported bases, and `skia-sync/main`.

Comment `/skia-sync-review` on the parent PR to publish append-only, exact-head
review evidence. The report is not an approval; review the recorded parent and native
SHAs before approving a landing. A trusted `skia-sync-review:v1` marker records that
pair and is the merge coordinator's evidence gate. Manual dispatch defaults to a
staged comment preview.

Comment `/skia-sync-merge` on the parent PR only after approving that evidence. The
coordinator treats the command as the approval, checks both original heads, preserves
the preceding release branch when necessary, merge-commits `mono/skia`, repins the
existing parent, and squash-merges it. It deliberately does not wait for post-repin PR
CI because the repin proves the merged native tree is identical to the reviewed tree;
base-branch CI is started and verified after landing.

Manual dispatch is plan-only by default. Applying is restricted to the trusted
`mono/SkiaSharp` `main` workflow revision. Failed or partial runs are resumable, but
every mutation rechecks the exact PR identity, base, and current head first. The
manual procedures in the review and merge skills remain the fallback when Actions is
unavailable.
