#!/usr/bin/env bash
# Category: Error Handling — Bare catch Blocks

cat << 'GUIDANCE'
## Category: Error Handling — Bare catch Blocks

### What to look for
catch (Exception) or catch blocks in binding/ and source/ that swallow exceptions
without logging, rethrowing, or handling a specific exception type.

### Good candidates
- catch blocks that call Thread.CurrentThread.Interrupt() with no rethrow
- catch blocks that silently return a default value with no logging
- catch (Exception) where a more specific exception type should be used
- catch blocks with only a comment and no action

### How to fix
Either:
1. Catch a more specific exception type (e.g., ThreadInterruptedException, IOException)
2. Add logging before swallowing: Log.Warn(ex); return default;
3. Rethrow after cleanup: catch (Exception) { cleanup(); throw; }

### ABI safety
Changing exception handling internals is ABI-safe.

### What NOT to flag
- catch (Exception ex) blocks that log ex properly — these are fine
- catch blocks that explicitly rethrow: throw; or throw ex;
- catch blocks in test code — tests may legitimately catch and assert
- The Monitor.Wait + Thread.CurrentThread.Interrupt() pattern in Android GL threading
  code (GLTextureView.cs): this is an intentional Java-port pattern for re-setting the
  thread interrupt flag. Only flag this if there is a clearly better .NET alternative
  (e.g., a CancellationToken-based wait) that the maintainers would accept
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Bare catch (Exception) blocks in binding/ and source/"
grep -rn 'catch\s*(Exception' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | shuf | head -20 || echo "None found"
echo ""
echo "### catch blocks without throw/rethrow (potential swallowed exceptions)"
for f in $(grep -rl 'catch' --include='*.cs' --exclude='*.generated.cs' --exclude-dir=obj binding/ source/ 2>/dev/null | shuf | head -15); do
    awk '/catch/{found=1; start=NR; block=""} found{block=block"\n"$0; if(/\}/){if(block !~ /throw/){print FILENAME":"start": "block}; found=0}}' "$f" 2>/dev/null
done | head -30 || echo "None found"
