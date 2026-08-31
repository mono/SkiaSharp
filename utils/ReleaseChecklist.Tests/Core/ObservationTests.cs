using ReleaseChecklist.Core;

namespace ReleaseChecklist.Tests.Core;

public class ObservationTests
{
	[Fact]
	public void EquivalentFieldsAreEqualAndRenderedInOrder()
	{
		var first = new ObservationBuilder()
			.Add("string", "text")
			.Add("integer", 42)
			.AddNull("null")
			.Add("boolean", true)
			.Build();
		var second = new ObservationBuilder()
			.Add("boolean", true)
			.AddNull("null")
			.Add("integer", 42)
			.Add("string", "text")
			.Build();

		Assert.Equal(first, second);
		Assert.Equal(
			"boolean=true, integer=42, null=null, string=text",
			first.ToString());

		var nullValue = new ObservationBuilder().AddNull("value").Build();
		var emptyValue = new ObservationBuilder().Add("value", "").Build();
		Assert.NotEqual(nullValue, emptyValue);
	}

	[Fact]
	public void DuplicateFieldIsRejected()
	{
		var builder = new ObservationBuilder().Add("value", 1);
		Assert.Throws<ArgumentException>(() => builder.Add("value", 2));
	}
}
