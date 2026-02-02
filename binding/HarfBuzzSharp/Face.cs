#nullable disable

using System;

namespace HarfBuzzSharp
{
	public unsafe class Face : NativeObject
	{
		private static readonly Lazy<Face> emptyFace = new Lazy<Face> (() => new StaticFace (HarfBuzzApi.hb_face_get_empty ()));

		public static Face Empty => emptyFace.Value;

		public Face (Blob blob, uint index)
			: this (blob, (int)index)
		{
		}

		public Face (Blob blob, int index)
			: this (IntPtr.Zero)
		{
			if (blob == null) {
				throw new ArgumentNullException (nameof (blob));
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException (nameof (index), "Index must be non negative.");
			}

			Handle = HarfBuzzApi.hb_face_create (blob.Handle, (uint)index);
		}

		public Face (GetTableDelegate getTable)
			: this (getTable, null)
		{
		}

		public Face (GetTableDelegate getTable, ReleaseDelegate destroy)
			: this (IntPtr.Zero)
		{
			if (getTable == null)
				throw new ArgumentNullException (nameof (getTable));

			Handle = HarfBuzzApi.hb_face_create_for_tables (
				DelegateProxies.ReferenceTableProxy,
				(void*)DelegateProxies.CreateMultiUserData (getTable, destroy, this),
				DelegateProxies.DestroyProxyForMulti);
		}

		internal Face (IntPtr handle)
			: base (handle)
		{
		}

		public int Index {
			get => (int)HarfBuzzApi.hb_face_get_index (Handle);
			set => HarfBuzzApi.hb_face_set_index (Handle, (uint)value);
		}

		public int UnitsPerEm {
			get => (int)HarfBuzzApi.hb_face_get_upem (Handle);
			set => HarfBuzzApi.hb_face_set_upem (Handle, (uint)value);
		}

		public int GlyphCount {
			get => (int)HarfBuzzApi.hb_face_get_glyph_count (Handle);
			set => HarfBuzzApi.hb_face_set_glyph_count (Handle, (uint)value);
		}

		public unsafe Tag[] Tables {
			get {
				uint tableCount;
				var count = HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &tableCount, null);
				var buffer = new Tag[count];
				fixed (void* ptr = buffer) {
					HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &count, (uint*)ptr);
				}
				return buffer;
			}
		}

		public Blob ReferenceTable (Tag table) =>
			new Blob (HarfBuzzApi.hb_face_reference_table (Handle, table));

		public bool IsImmutable => HarfBuzzApi.hb_face_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_face_make_immutable (Handle);

		/// <summary>
		/// Gets the name string from the face's OpenType 'name' table.
		/// </summary>
		/// <param name="nameId">The name ID to retrieve.</param>
		/// <returns>The name string, or an empty string if not found.</returns>
		public string GetName (OpenTypeNameId nameId) =>
			GetName (nameId, Language.Default);

		/// <summary>
		/// Gets the name string from the face's OpenType 'name' table.
		/// </summary>
		/// <param name="nameId">The name ID to retrieve.</param>
		/// <param name="language">The language for the name.</param>
		/// <returns>The name string, or an empty string if not found.</returns>
		public string GetName (OpenTypeNameId nameId, Language language)
		{
			if (language == null)
				throw new ArgumentNullException (nameof (language));

			// First call to get the length
			uint length = 0;
			HarfBuzzApi.hb_ot_name_get_utf16 (Handle, nameId, language.Handle, &length, null);

			if (length == 0)
				return string.Empty;

			// Allocate buffer and get the string
			length++; // Add space for null terminator
			var buffer = stackalloc ushort[(int)length];
			HarfBuzzApi.hb_ot_name_get_utf16 (Handle, nameId, language.Handle, &length, buffer);

			return new string ((char*)buffer, 0, (int)length);
		}

		/// <summary>
		/// Tries to get the name string from the face's OpenType 'name' table.
		/// </summary>
		/// <param name="nameId">The name ID to retrieve.</param>
		/// <param name="name">The name string, or null if not found.</param>
		/// <returns>True if the name was found, false otherwise.</returns>
		public bool TryGetName (OpenTypeNameId nameId, out string name) =>
			TryGetName (nameId, Language.Default, out name);

		/// <summary>
		/// Tries to get the name string from the face's OpenType 'name' table.
		/// </summary>
		/// <param name="nameId">The name ID to retrieve.</param>
		/// <param name="language">The language for the name.</param>
		/// <param name="name">The name string, or null if not found.</param>
		/// <returns>True if the name was found, false otherwise.</returns>
		public bool TryGetName (OpenTypeNameId nameId, Language language, out string name)
		{
			if (language == null)
				throw new ArgumentNullException (nameof (language));

			// First call to get the length
			uint length = 0;
			HarfBuzzApi.hb_ot_name_get_utf16 (Handle, nameId, language.Handle, &length, null);

			if (length == 0) {
				name = null;
				return false;
			}

			// Allocate buffer and get the string
			length++; // Add space for null terminator
			var buffer = stackalloc ushort[(int)length];
			HarfBuzzApi.hb_ot_name_get_utf16 (Handle, nameId, language.Handle, &length, buffer);

			name = new string ((char*)buffer, 0, (int)length);
			return true;
		}

		/// <summary>
		/// Gets all name entries from the face's OpenType 'name' table.
		/// </summary>
		/// <returns>An array of name entries.</returns>
		public OpenTypeNameEntry[] GetAllNameEntries ()
		{
			uint count = 0;
			var entries = HarfBuzzApi.hb_ot_name_list_names (Handle, &count);

			if (count == 0 || entries == null)
				return Array.Empty<OpenTypeNameEntry> ();

			var result = new OpenTypeNameEntry[count];
			for (int i = 0; i < count; i++)
				result[i] = entries[i];

			return result;
		}

		/// <summary>
		/// Gets all script tags supported by the specified OpenType layout table.
		/// </summary>
		/// <param name="tableTag">The layout table to query (GSUB or GPOS).</param>
		/// <returns>An array of script tags.</returns>
		public Tag[] GetOpenTypeLayoutScriptTags (OpenTypeLayoutTableTag tableTag)
		{
			// First call to get the count
			uint count = 0;
			var total = HarfBuzzApi.hb_ot_layout_table_get_script_tags (Handle, (uint)tableTag, 0, &count, null);

			if (total == 0)
				return Array.Empty<Tag> ();

			// Allocate and get all tags
			var buffer = new Tag[total];
			count = total;
			fixed (Tag* ptr = buffer) {
				HarfBuzzApi.hb_ot_layout_table_get_script_tags (Handle, (uint)tableTag, 0, &count, (uint*)ptr);
			}

			return buffer;
		}

		/// <summary>
		/// Gets all feature tags supported by the specified OpenType layout table.
		/// </summary>
		/// <param name="tableTag">The layout table to query (GSUB or GPOS).</param>
		/// <returns>An array of feature tags.</returns>
		public Tag[] GetOpenTypeLayoutFeatureTags (OpenTypeLayoutTableTag tableTag)
		{
			// First call to get the count
			uint count = 0;
			var total = HarfBuzzApi.hb_ot_layout_table_get_feature_tags (Handle, (uint)tableTag, 0, &count, null);

			if (total == 0)
				return Array.Empty<Tag> ();

			// Allocate and get all tags
			var buffer = new Tag[total];
			count = total;
			fixed (Tag* ptr = buffer) {
				HarfBuzzApi.hb_ot_layout_table_get_feature_tags (Handle, (uint)tableTag, 0, &count, (uint*)ptr);
			}

			return buffer;
		}

		/// <summary>
		/// Tries to get the name IDs for an OpenType layout feature.
		/// </summary>
		/// <param name="tableTag">The layout table (GSUB or GPOS).</param>
		/// <param name="featureIndex">The index of the feature in the table.</param>
		/// <param name="nameIds">The name IDs for the feature if found.</param>
		/// <returns>True if the feature has name IDs, false otherwise.</returns>
		public bool TryGetOpenTypeLayoutFeatureNameIds (OpenTypeLayoutTableTag tableTag, int featureIndex, out OpenTypeFeatureNameIds nameIds)
		{
			OpenTypeNameId labelId, tooltipId, sampleId, firstParamId;
			uint numParams;

			var result = HarfBuzzApi.hb_ot_layout_feature_get_name_ids (
				Handle, (uint)tableTag, (uint)featureIndex,
				&labelId, &tooltipId, &sampleId, &numParams, &firstParamId);

			if (!result) {
				nameIds = default;
				return false;
			}

			nameIds = new OpenTypeFeatureNameIds {
				LabelId = labelId,
				TooltipId = tooltipId,
				SampleId = sampleId,
				NumNamedParameters = (int)numParams,
				FirstParamId = firstParamId
			};
			return true;
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_face_destroy (Handle);
			}
		}

		private class StaticFace : Face
		{
			public StaticFace (IntPtr handle)
				: base (handle)
			{
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
