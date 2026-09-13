namespace Fixtures;

public sealed class Widget
{
	public Widget(string name)
	{
	}

	public string Name { get; }

	public static Widget Create(string name) => new(name);

	public static Widget Create(int value) => new(value.ToString());

	public static bool operator ==(Widget? left, Widget? right) => Equals(left, right);

	public static bool operator !=(Widget? left, Widget? right) => !Equals(left, right);
}
