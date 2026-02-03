# PR Templates

## Investigation Phase Template

Use when creating a draft PR to track investigation. **Update this often** as you work.

```markdown
## Investigation: #NNNN - [Title]

### Status: 🔍 Investigating

### Issue Summary
- **Title:** [title]
- **Symptoms:** [what happens]
- **Platform:** [OS, arch, .NET version, SkiaSharp version]
- **Reproduction:** [steps or code]

### Related Issues
| Issue | Title | Why Similar | Status |
|-------|-------|-------------|--------|
| #XXXX | [title] | Same platform, same error message | Open |
| #YYYY | [title] | Same root cause identified in comments | Closed |

**Links from related issues:**
- [Link to external project issue] - [why relevant]
- [Link to another related discussion] - [why relevant]

### Reproduction Results
| Environment | SkiaSharp Version | Result |
|-------------|-------------------|--------|
| [Platform from issue] | [reported version] | ❌ Crashes |
| [Platform from issue] | [last working version] | ✅ Works |
| [Different platform] | [reported version] | ✅ Works |

### Current Hypothesis
[What you think the root cause is based on evidence]

### Investigation Plan
- [x] Fetch issue and extract details
- [x] Search for related issues (found #XXXX, #YYYY)
- [x] Read all comments on related issues
- [x] Reproduce on target platform
- [ ] Compare working vs broken versions
- [ ] Check native library dependencies
- [ ] Identify fix location

### Progress Log
| Action | Result |
|--------|--------|
| Searched related issues | Found #XXXX (same platform), #YYYY (same error) |
| Read #XXXX comments | User found workaround: [description] |
| Tested on [target platform] | Reproduced crash |
| Tested [older version] | Works - regression identified |

### Alternatives Tried
| Approach | Result | Next Step |
|----------|--------|-----------|
| [First attempt] | Didn't reproduce | Try [alternative] |
| [Second attempt] | Reproduced! | Continue investigation |
```

## Final PR Template

Use when fix is complete and ready for review:

```markdown
## Fix: #NNNN - [Title]

### Summary
[One paragraph explaining the bug and fix]

### Root Cause
[Technical explanation of what was wrong]

### Solution
[What was changed to fix it]

### Testing
- [x] Added regression test: `Issue_NNNN_MethodDoesNotCrash`
- [x] Verified fix on [target platform]
- [x] All existing tests pass

### Related Issues
Fixes #NNNN
Also fixes #XXXX (same root cause)
Related to #YYYY (similar symptom, different cause)
```

## Linking Multiple Issues

If the PR fixes multiple issues, link them ALL in the description:

```markdown
Fixes #NNNN
Fixes #XXXX
Fixes #YYYY
```
