using System.Text.RegularExpressions;

namespace DocsSamplesApp;

/// <summary>
/// Registers all demo pages as Shell routes for URI-based navigation.
/// Routes use a hierarchical structure like /basics/simple-circle.
/// </summary>
public static class RouteRegistration
{
    /// <summary>
    /// Registers all ContentPage types in the assembly as routes.
    /// Routes are hierarchical: /category/page-name (e.g., /basics/simple-circle)
    /// Category home pages use just the category name (e.g., /basics)
    /// </summary>
    public static void RegisterRoutes()
    {
        var assembly = typeof(RouteRegistration).Assembly;
        var pageTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ContentPage)) &&
                        !t.IsAbstract &&
                        t != typeof(HomePage) && // Main home page is in Shell
                        t.Namespace?.StartsWith("DocsSamplesApp") == true);

        foreach (var pageType in pageTypes)
        {
            var route = GetRoute(pageType);
            Routing.RegisterRoute(route, pageType);
        }
    }

    /// <summary>
    /// Gets the route for a given page type.
    /// Converts namespace + class name to kebab-case route.
    /// e.g., DocsSamplesApp.Basics.SimpleCirclePage -> basics/simple-circle
    /// e.g., DocsSamplesApp.Basics.BasicsHomePage -> basics
    /// </summary>
    public static string GetRoute(Type pageType)
    {
        // Get category from namespace (e.g., "Basics" from "DocsSamplesApp.Basics")
        var ns = pageType.Namespace ?? "";
        var category = ns.Replace("DocsSamplesApp.", "").Replace("DocsSamplesApp", "");
        
        // Check if this is a category home page (e.g., BasicsHomePage)
        if (pageType.Name.EndsWith("HomePage"))
        {
            return category.ToLowerInvariant();
        }
        
        // Get page name without "Page" suffix and convert to kebab-case
        var pageName = pageType.Name;
        if (pageName.EndsWith("Page"))
            pageName = pageName[..^4];
        
        var kebabPage = ToKebabCase(pageName);
        
        // Build route
        if (string.IsNullOrEmpty(category))
            return kebabPage;
        
        return $"{category.ToLowerInvariant()}/{kebabPage}";
    }

    /// <summary>
    /// Converts PascalCase to kebab-case.
    /// e.g., SimpleCircle -> simple-circle
    /// </summary>
    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Insert hyphen before uppercase letters (except at start)
        var result = Regex.Replace(input, "(?<!^)([A-Z])", "-$1");
        return result.ToLowerInvariant();
    }
}
