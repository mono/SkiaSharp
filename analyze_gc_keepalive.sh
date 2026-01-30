#!/bin/bash
# Script to help identify methods in SkiaSharp that need GC.KeepAlive calls
# This script analyzes C# files to find P/Invoke calls that use .Handle

echo "=== SkiaSharp GC.KeepAlive Analysis ==="
echo ""

cd binding/SkiaSharp

echo "Files with P/Invoke calls using .Handle:"
echo "----------------------------------------"

for file in *.cs; do
  if [ ! -f "$file" ]; then
    continue
  fi
  
  # Count SkiaApi calls with .Handle
  handle_count=$(grep "SkiaApi\." "$file" 2>/dev/null | grep -c "\.Handle" || echo "0")
  
  if [ "$handle_count" -gt 0 ]; then
    # Check if file already has GC.KeepAlive calls
    keepalive_count=$(grep -c "GC\.KeepAlive" "$file" 2>/dev/null || echo "0")
    
    if [ "$keepalive_count" -eq 0 ]; then
      status="❌ NO GC.KeepAlive"
    elif [ "$keepalive_count" -lt "$handle_count" ]; then
      status="⚠️  PARTIAL ($keepalive_count/$handle_count)"
    else
      status="✅ HAS GC.KeepAlive ($keepalive_count)"
    fi
    
    printf "%-40s %3d calls | %s\n" "$file" "$handle_count" "$status"
  fi
done | sort -t'|' -k2 -rn

echo ""
echo "=== Detailed Analysis by File ==="
echo ""

# Function to extract method signatures with SkiaApi calls
analyze_file() {
  local file=$1
  echo "File: $file"
  echo "-------------------------------------------"
  
  # Find methods that call SkiaApi with .Handle
  awk '
    /public.*\(/ { method=$0; in_method=1; brace_count=0 }
    in_method && /{/ { brace_count++ }
    in_method && /}/ { 
      brace_count--
      if (brace_count == 0) { in_method=0 }
    }
    in_method && /SkiaApi\./ && /\.Handle/ {
      if (!printed[method]) {
        # Clean up method signature
        gsub(/^\t+/, "", method)
        print "  " method
        printed[method] = 1
      }
    }
  ' "$file"
  
  echo ""
}

# Analyze top priority files
priority_files=("SKPath.cs" "SKFont.cs" "SKImageFilter.cs" "SKTextBlob.cs" "SKShader.cs" "SKSurface.cs" "SKRegion.cs")

for file in "${priority_files[@]}"; do
  if [ -f "$file" ]; then
    handle_count=$(grep "SkiaApi\." "$file" 2>/dev/null | grep -c "\.Handle" || echo "0")
    keepalive_count=$(grep -c "GC\.KeepAlive" "$file" 2>/dev/null || echo "0")
    
    if [ "$handle_count" -gt 0 ] && [ "$keepalive_count" -lt "$handle_count" ]; then
      analyze_file "$file"
    fi
  fi
done

echo ""
echo "=== Summary ==="
echo ""

total_files=$(ls *.cs 2>/dev/null | wc -l)
files_with_pinvoke=$(grep -l "SkiaApi\." *.cs 2>/dev/null | wc -l)
files_with_keepalive=$(grep -l "GC\.KeepAlive" *.cs 2>/dev/null | wc -l)

echo "Total .cs files: $total_files"
echo "Files with P/Invoke: $files_with_pinvoke"
echo "Files with GC.KeepAlive: $files_with_keepalive"
echo "Files needing work: $((files_with_pinvoke - files_with_keepalive))"

echo ""
echo "=== Next Steps ==="
echo "1. Review methods listed above"
echo "2. For each method with reference type parameters:"
echo "   - Add GC.KeepAlive(param) after the SkiaApi call"
echo "   - Remember to add for ALL reference type parameters"
echo "3. Test compilation after changes"
echo "4. Run unit tests to verify no regressions"
