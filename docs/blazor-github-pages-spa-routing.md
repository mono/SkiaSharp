# Blazor WebAssembly SPA Routing on GitHub Pages

> Comprehensive guide for hosting Blazor WASM apps on GitHub Pages subdirectories.
> Based on [Microsoft Learn documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages) and the [spa-github-pages](https://github.com/rafrex/spa-github-pages) approach.

## The Problem

Blazor WebAssembly apps use client-side routing. When a user navigates directly to a route like `/dashboard/issues`, GitHub Pages returns a 404 because no `issues` file exists. The solution involves:

1. Intercepting 404s and redirecting to `index.html`
2. Preserving the original URL so Blazor can route correctly
3. Handling the `<base href>` for apps in subdirectories

## Quick Start

For an app at `https://username.github.io/repo-name/`:

### 1. Add 404.html to wwwroot

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Redirecting...</title>
    <script type="text/javascript">
      // segmentCount = 1 for /repo-name/ (one segment to preserve)
      var segmentCount = 1;
      var l = window.location;
      l.replace(
        l.protocol + '//' + l.hostname + (l.port ? ':' + l.port : '') +
        l.pathname.split('/').slice(0, 1 + segmentCount).join('/') + '/?p=/' +
        l.pathname.slice(1).split('/').slice(segmentCount).join('/').replace(/&/g, '~and~') +
        (l.search ? '&q=' + l.search.slice(1).replace(/&/g, '~and~') : '') +
        l.hash
      );
    </script>
</head>
<body></body>
</html>
```

### 2. Add script to index.html `<head>`

```html
<head>
    <!-- BEFORE base href and other tags -->
    <script type="text/javascript">
        (function (l) {
            if (l.search) {
                var q = {};
                l.search.slice(1).split('&').forEach(function (v) {
                    var a = v.split('=');
                    q[a[0]] = a.slice(1).join('=').replace(/~and~/g, '&');
                });
                if (q.p !== undefined) {
                    window.history.replaceState(null, null,
                        l.pathname.slice(0, -1) + (q.p || '') +
                        (q.q ? ('?' + q.q) : '') +
                        l.hash
                    );
                }
            }
        }(window.location))
    </script>
    <base href="/repo-name/" />
    <!-- rest of head -->
</head>
```

### 3. Set base href in CI/CD

```yaml
- name: Set base href for GitHub Pages
  run: sed -i 's|<base href="/" />|<base href="/repo-name/" />|' ./publish/wwwroot/index.html
```

## Nested Subdirectories

For apps at deeper paths like `https://username.github.io/org/app/`:

### segmentCount Calculation

| URL Path | segmentCount |
|----------|--------------|
| `/repo-name/` | 1 |
| `/org/app/` | 2 |
| `/org/team/app/` | 3 |

### Root 404.html Issue

**Critical**: GitHub Pages serves the 404.html from the closest parent directory. If there's already a 404.html at the repository root (e.g., for documentation), it will intercept requests to your app's subdirectory.

**Solution**: Update the root 404.html to handle your app's paths:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Page Not Found</title>
    <script type="text/javascript">
        (function() {
            var path = window.location.pathname;
            // Handle SPA routing for /org/app/ subdirectory
            if (path.startsWith('/org/app/') && path !== '/org/app/') {
                var segmentCount = 2;
                var l = window.location;
                l.replace(
                    l.protocol + '//' + l.hostname + (l.port ? ':' + l.port : '') +
                    l.pathname.split('/').slice(0, 1 + segmentCount).join('/') + '/?p=/' +
                    l.pathname.slice(1).split('/').slice(segmentCount).join('/').replace(/&/g, '~and~') +
                    (l.search ? '&q=' + l.search.slice(1).replace(/&/g, '~and~') : '') +
                    l.hash
                );
            }
        })();
    </script>
</head>
<body>
    <h1>Page Not Found</h1>
    <p><a href="/org/">Return to main site</a></p>
</body>
</html>
```

## URL Path Rules

**This is the #1 source of bugs!**

### NavigationManager.NavigateTo()

```csharp
// ❌ WRONG - Goes to site root /issues (bypasses base href)
Navigation.NavigateTo("/issues");

// ✅ CORRECT - Relative to base href
Navigation.NavigateTo("issues");
Navigation.NavigateTo("./issues");
```

### Anchor tags and NavLink

```html
<!-- ❌ WRONG - Goes to site root -->
<a href="/issues">Issues</a>

<!-- ✅ CORRECT - Relative to base href -->
<a href="issues">Issues</a>
<NavLink href="issues">Issues</NavLink>
```

### @page directive (routes)

```csharp
// ✅ Routes DO use leading slash - this is correct!
@page "/issues"
@page "/pull-requests"
```

### Building URLs programmatically

```csharp
// ❌ WRONG
var url = "/issues" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
Navigation.NavigateTo(url);

// ✅ CORRECT
var url = "issues" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
Navigation.NavigateTo(url, replace: true);
```

## How the Redirect Flow Works

1. User navigates to `https://example.github.io/org/app/issues`
2. GitHub returns 404, serves `404.html`
3. 404.html script redirects to `https://example.github.io/org/app/?p=/issues`
4. GitHub serves `index.html` (exists at `/org/app/`)
5. index.html script sees `?p=/issues` in query string
6. Script calls `history.replaceState` to restore URL to `/org/app/issues`
7. Blazor starts, sees URL `/org/app/issues`
8. Blazor Router extracts route `/issues` (after removing base `/org/app/`)
9. Router matches `/issues` to Issues page component

## Debugging Checklist

When navigation breaks:

- [ ] Check NavigateTo calls for leading slashes
- [ ] Check anchor hrefs for leading slashes
- [ ] Verify `<base href>` has trailing slash: `<base href="/org/app/">`
- [ ] Verify 404.html has correct segmentCount
- [ ] Check if root 404.html is intercepting (look at network tab)
- [ ] Clear browser cache and hard refresh
- [ ] Check browser console for navigation errors

## Common Symptoms

| Symptom | Likely Cause |
|---------|--------------|
| Clicking link goes to `example.github.io/issues` instead of `example.github.io/org/app/issues` | Leading slash in NavigateTo or href |
| Direct URL access shows 404 page | 404.html not deployed or wrong segmentCount |
| Page loads then redirects to home | NavigateTo with leading slash in OnInitialized |
| Page loads but wrong route matched | Base href missing or wrong |

## CI/CD Workflow Example

```yaml
- name: Publish
  run: dotnet publish MyApp -c Release -o ./publish

- name: Set base href
  run: sed -i 's|<base href="/" />|<base href="/org/app/" />|' ./publish/wwwroot/index.html

- name: Add .nojekyll
  run: touch ./publish/wwwroot/.nojekyll

- name: Deploy to GitHub Pages
  uses: peaceiris/actions-gh-pages@v4
  with:
    github_token: ${{ secrets.GITHUB_TOKEN }}
    publish_dir: ./publish/wwwroot
    publish_branch: gh-pages
    destination_dir: app  # if deploying to subdirectory
    keep_files: true
```

## References

- [MS Learn: GitHub Pages deployment](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages)
- [MS Learn: App base path](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/app-base-path)
- [spa-github-pages](https://github.com/rafrex/spa-github-pages)
- [Microsoft's Blazor sample](https://github.com/dotnet/blazor-samples/tree/main/BlazorWebAssemblyXrefGenerator)
