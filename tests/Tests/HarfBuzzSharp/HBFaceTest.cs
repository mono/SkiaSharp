using System;
using System.IO;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFaceTest : HBTest
	{
		// Variable font helpers
		private Face CreateVariableFace ()
		{
			using var blob = Blob.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			return new Face (blob, 0);
		}

		// US1: Query Variable Font Axes

		[SkippableFact]
		public void CanGetVariationAxisCount ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.VariationAxisCount > 0);
		}

		[SkippableFact]
		public void CanGetVariationAxisInfos ()
		{
			using var face = CreateVariableFace ();
			var axes = face.VariationAxisInfos;
			Assert.NotEmpty (axes);

			// Distortable.ttf should have at least one axis
			var axis = axes[0];
			Assert.NotEqual ((uint)0, axis.Tag);
			Assert.True (axis.MinValue <= axis.DefaultValue);
			Assert.True (axis.DefaultValue <= axis.MaxValue);
		}

		[SkippableFact]
		public void VariationAxisCountIsZeroForStaticFont ()
		{
			using var face = new Face (Blob, 0);
			Assert.Equal (0, face.VariationAxisCount);
		}

		[SkippableFact]
		public void TryFindVariationAxisReturnsTrueForExistingAxis ()
		{
			using var face = CreateVariableFace ();
			var axes = face.VariationAxisInfos;
			Assert.NotEmpty (axes);

			Tag axisTag = axes[0].Tag;
			var found = face.TryFindVariationAxis (axisTag, out var axisInfo);
			Assert.True (found);
			Assert.Equal (axes[0].Tag, axisInfo.Tag);
		}

		[SkippableFact]
		public void TryFindVariationAxisReturnsFalseForMissingAxis ()
		{
			using var face = CreateVariableFace ();
			// Use a tag that is unlikely to exist
			var found = face.TryFindVariationAxis (Tag.Parse ("zzzz"), out _);
			Assert.False (found);
		}

		// US3: Named Instances

		[SkippableFact]
		public void VariationAxisCountMatchesArrayLength ()
		{
			using var face = CreateVariableFace ();
			var axes = face.VariationAxisInfos;
			Assert.Equal (face.VariationAxisCount, axes.Length);
		}

		[SkippableFact]
		public void SpanGetVariationAxisInfosMatchesArrayVersion ()
		{
			using var face = CreateVariableFace ();
			var arrayResult = face.VariationAxisInfos;
			Assert.NotEmpty (arrayResult);

			var spanBuffer = new OpenTypeVarAxisInfo[face.VariationAxisCount];
			var written = face.GetVariationAxisInfos (spanBuffer);
			Assert.Equal (arrayResult.Length, written);

			for (int i = 0; i < arrayResult.Length; i++) {
				Assert.Equal (arrayResult[i].Tag, spanBuffer[i].Tag);
				Assert.Equal (arrayResult[i].MinValue, spanBuffer[i].MinValue);
				Assert.Equal (arrayResult[i].DefaultValue, spanBuffer[i].DefaultValue);
				Assert.Equal (arrayResult[i].MaxValue, spanBuffer[i].MaxValue);
			}
		}

		[SkippableFact]
		public void SpanGetVariationAxisInfosWithOversizedBuffer ()
		{
			using var face = CreateVariableFace ();
			var axisCount = face.VariationAxisCount;
			Assert.True (axisCount > 0);

			// Pass a buffer larger than needed
			var spanBuffer = new OpenTypeVarAxisInfo[axisCount + 5];
			var written = face.GetVariationAxisInfos (spanBuffer);
			Assert.Equal (axisCount, written);
		}

		[SkippableFact]
		public void CanGetNamedInstanceCount ()
		{
			using var face = CreateVariableFace ();
			// Distortable.ttf has 3 named instances
			Assert.True (face.NamedInstanceCount > 0);
		}

		[SkippableFact]
		public void NamedInstanceDesignCoordsCountMatchesAxisCount ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.NamedInstanceCount > 0);

			// Per HarfBuzz docs, design coords count equals the number of axes
			var coordsCount = face.GetNamedInstanceDesignCoordsCount (0);
			Assert.Equal (face.VariationAxisCount, coordsCount);
		}

		[SkippableFact]
		public void NamedInstanceDesignCoordsArrayLengthMatchesCount ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.NamedInstanceCount > 0);

			var coordsCount = face.GetNamedInstanceDesignCoordsCount (0);
			var coords = face.GetNamedInstanceDesignCoords (0);
			Assert.Equal (coordsCount, coords.Length);
			Assert.NotEmpty (coords);
		}

		[SkippableFact]
		public void SpanGetNamedInstanceDesignCoordsMatchesArrayVersion ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.NamedInstanceCount > 0);

			var arrayResult = face.GetNamedInstanceDesignCoords (0);
			Assert.NotEmpty (arrayResult);

			var spanBuffer = new float[face.GetNamedInstanceDesignCoordsCount (0)];
			var written = face.GetNamedInstanceDesignCoords (0, spanBuffer);
			Assert.Equal (arrayResult.Length, written);

			for (int i = 0; i < arrayResult.Length; i++)
				Assert.Equal (arrayResult[i], spanBuffer[i]);
		}

		[SkippableFact]
		public void EachNamedInstanceHasDesignCoords ()
		{
			using var face = CreateVariableFace ();
			var instanceCount = face.NamedInstanceCount;
			Assert.True (instanceCount > 0);

			for (int i = 0; i < instanceCount; i++) {
				var coords = face.GetNamedInstanceDesignCoords (i);
				Assert.NotEmpty (coords);
				Assert.Equal (face.VariationAxisCount, coords.Length);
			}
		}

		[SkippableFact]
		public void NamedInstanceSubfamilyNameIdIsValid ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.NamedInstanceCount > 0);

			var nameId = face.GetNamedInstanceSubfamilyNameId (0);
			// Name IDs are unsigned; a valid named instance should have a non-zero subfamily name ID
			Assert.True ((uint)nameId > 0);
		}

		[SkippableFact]
		public void NamedInstancePostScriptNameIdDoesNotThrow ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.NamedInstanceCount > 0);

			// PostScript name ID may be 0xFFFF (invalid) for some fonts, but it should not throw
			var nameId = face.GetNamedInstancePostScriptNameId (0);
			// Just verify it returns without error
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForSubfamilyNameId ()
		{
			using var face = CreateVariableFace ();
			Assert.Throws<ArgumentOutOfRangeException> (() => face.GetNamedInstanceSubfamilyNameId (-1));
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForPostScriptNameId ()
		{
			using var face = CreateVariableFace ();
			Assert.Throws<ArgumentOutOfRangeException> (() => face.GetNamedInstancePostScriptNameId (-1));
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForDesignCoords ()
		{
			using var face = CreateVariableFace ();
			Assert.Throws<ArgumentOutOfRangeException> (() => face.GetNamedInstanceDesignCoords (-1));
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForDesignCoordsCount ()
		{
			using var face = CreateVariableFace ();
			Assert.Throws<ArgumentOutOfRangeException> (() => face.GetNamedInstanceDesignCoordsCount (-1));
		}

		[SkippableFact]
		public void NegativeInstanceIndexThrowsForDesignCoordsSpan ()
		{
			using var face = CreateVariableFace ();
			Assert.Throws<ArgumentOutOfRangeException> (() => face.GetNamedInstanceDesignCoords (-1, new float[1]));
		}

		[SkippableFact]
		public void NamedInstanceCountIsZeroForStaticFont ()
		{
			using var face = new Face (Blob, 0);
			Assert.Equal (0, face.NamedInstanceCount);
		}

		[SkippableFact]
		public void HasVariationDataIsTrueForVariableFont ()
		{
			using var face = CreateVariableFace ();
			Assert.True (face.HasVariationData);
		}

		[SkippableFact]
		public void HasVariationDataIsFalseForStaticFont ()
		{
			using var face = new Face (Blob, 0);
			Assert.False (face.HasVariationData);
		}

		[SkippableFact]
		public void ShouldHaveGlyphCount()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(1147, face.GlyphCount);
			}
		}

		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var face = new Face(Blob, 0))
			{
				face.MakeImmutable();

				Assert.True(face.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldHaveIndex()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(0, face.Index);
			}
		}

		[SkippableFact]
		public void ShouldHaveUnitsPerEm()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(2048, face.UnitsPerEm);
			}
		}

		[SkippableFact]
		public void ShouldHaveTableTags()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(20, face.Tables.Length);
			}
		}

		[SkippableFact]
		public void ShouldReferenceTable()
		{
			using (var face = new Face(Blob, 0))
			using (var tableBlob = face.ReferenceTable(Tag.Parse("post")))
			{
				Assert.Equal(13378, tableBlob.Length);
			}
		}

		[SkippableFact]
		public void ShouldCreateWithTableFunc()
		{
			var tag = Tag.Parse("kern");

			using (var face = new Face((f, t) => Face.ReferenceTable(t)))
			{
				var blob = face.ReferenceTable(tag);

				Assert.Equal(Face.ReferenceTable(tag).Handle, blob.Handle);
			}
		}

		[SkippableFact]
		public void DelegateBasedConstructionSucceededs()
		{
			var didReference = 0;
			var didDestroy = 0;

			var tag = Tag.Parse("kern");

			Face face = null;
			face = new Face(
				(f, t) =>
				{
					Assert.Equal(face, f);

					didReference++;
					return Face.ReferenceTable(t);
				},
				() =>
				{
					didDestroy++;
				});

			Assert.Equal(0, didReference);
			Assert.Equal(0, didDestroy);

			var blob1 = face.ReferenceTable(tag);

			Assert.Equal(1, didReference);
			Assert.Equal(0, didDestroy);
			Assert.NotNull(blob1);

			var blob2 = face.ReferenceTable(tag);

			Assert.Equal(2, didReference);
			Assert.Equal(0, didDestroy);
			Assert.NotNull(blob2);

			Assert.Equal(blob1.Handle, blob2.Handle);

			face.Dispose();

			Assert.Equal(2, didReference);
			Assert.Equal(1, didDestroy);
		}


		[SkippableFact]
		public void EmptyFacesAreExactlyTheSameInstance()
		{
			var emptyFace1 = Face.Empty;
			var emptyFace2 = Face.Empty;

			Assert.Equal(emptyFace1, emptyFace2);
			Assert.Equal(emptyFace1.Handle, emptyFace2.Handle);
			Assert.Same(emptyFace1, emptyFace2);
		}

		[SkippableFact]
		public void EmptyFacesAreNotDisposed()
		{
			var emptyFace = Face.Empty;
			emptyFace.Dispose();

			Assert.False(emptyFace.IsDisposed());
			Assert.NotEqual(IntPtr.Zero, emptyFace.Handle);
		}
	}
}
