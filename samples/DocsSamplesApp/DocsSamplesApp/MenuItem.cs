namespace DocsSamplesApp;

/// <summary>
/// Represents a menu item in the navigation pages.
/// </summary>
public class MenuItem
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public Type? PageType { get; set; }
}

/// <summary>
/// Represents a section of menu items with a header.
/// </summary>
public class MenuSection : List<MenuItem>
{
    public string Name { get; set; } = string.Empty;

    public MenuSection() { }

    public MenuSection(string name) => Name = name;
}
