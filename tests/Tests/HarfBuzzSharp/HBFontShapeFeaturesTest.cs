using System;
using System.Collections;
using System.Collections.Generic;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	// Equivalence coverage for the Font.Shape defensive-copy optimization: when the caller
	// already passes a Feature[] we pin it directly instead of calling features.ToArray().
	// These tests prove the shaped result is identical whether the features arrive as an
	// array (fast path) or as a non-array IReadOnlyList<Feature> (ToArray fallback).
	public class HBFontShapeFeaturesTest : HBTest
	{
		private const string Text = "hello world 1234";

		// A non-array IReadOnlyList<Feature> so `features as Feature[]` returns null and the
		// original ToArray() fallback path is exercised.
		private sealed class NonArrayFeatureList : IReadOnlyList<Feature>
		{
			private readonly List<Feature> inner;

			public NonArrayFeatureList (IEnumerable<Feature> items) => inner = new List<Feature> (items);

			public Feature this[int index] => inner[index];

			public int Count => inner.Count;

			public IEnumerator<Feature> GetEnumerator () => inner.GetEnumerator ();

			IEnumerator IEnumerable.GetEnumerator () => inner.GetEnumerator ();
		}

		private static (GlyphInfo[] infos, GlyphPosition[] positions) ShapeWith (IReadOnlyList<Feature> features) =>
			ShapeText (Text, features);

		private static (GlyphInfo[] infos, GlyphPosition[] positions) ShapeText (string text, IReadOnlyList<Feature> features)
		{
			using var buffer = new Buffer ();
			buffer.AddUtf8 (text);
			buffer.GuessSegmentProperties ();
			Font.Shape (buffer, features, null);
			return (buffer.GlyphInfos, buffer.GlyphPositions);
		}

		private static void AssertIdentical (
			(GlyphInfo[] infos, GlyphPosition[] positions) expected,
			(GlyphInfo[] infos, GlyphPosition[] positions) actual)
		{
			Assert.Equal (expected.infos, actual.infos);
			Assert.Equal (expected.positions, actual.positions);
		}

		public static IEnumerable<object[]> FeatureSets ()
		{
			yield return new object[] { Array.Empty<Feature> () };
			yield return new object[] { new[] { new Feature (new Tag ('k', 'e', 'r', 'n'), 1) } };
			yield return new object[] {
				new[] {
					new Feature (new Tag ('k', 'e', 'r', 'n'), 1),
					new Feature (new Tag ('l', 'i', 'g', 'a'), 0),
					new Feature (new Tag ('c', 'a', 'l', 't'), 1),
				}
			};
		}

		[Theory]
		[MemberData (nameof (FeatureSets))]
		public void ArrayAndNonArrayFeaturesProduceIdenticalShaping (Feature[] features)
		{
			var fromArray = ShapeWith (features);
			var fromList = ShapeWith (new NonArrayFeatureList (features));

			AssertIdentical (fromArray, fromList);
		}

		[Fact]
		public void NullFeaturesProduceIdenticalShaping ()
		{
			// The `params Feature[]` overload with no args passes an empty array; a null list
			// takes neither path. Both must match a plain no-feature shape.
			var fromEmptyArray = ShapeWith (Array.Empty<Feature> ());
			var fromNull = ShapeWith ((IReadOnlyList<Feature>)null);

			AssertIdentical (fromEmptyArray, fromNull);
		}

		[Fact]
		public void EquivalenceTestCatchesWrongResult ()
		{
			// Guard: the comparison must actually be sensitive to the shaped output. Shaping
			// different text through the same path must NOT compare equal — proving that an
			// accidental behaviour change in the optimized path would be caught.
			var features = new[] { new Feature (new Tag ('k', 'e', 'r', 'n'), 1) };
			var hello = ShapeText ("hello world 1234", features);
			var other = ShapeText ("goodbye 5678", features);

			var different =
				!AreEqual (hello.infos, other.infos) ||
				!AreEqual (hello.positions, other.positions);

			Assert.True (different, "Expected different text to yield different shaping.");
		}

		private static bool AreEqual<T> (T[] a, T[] b)
		{
			if (a.Length != b.Length)
				return false;
			for (var i = 0; i < a.Length; i++) {
				if (!EqualityComparer<T>.Default.Equals (a[i], b[i]))
					return false;
			}
			return true;
		}
	}
}
