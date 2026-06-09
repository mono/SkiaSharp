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
			GC.KeepAlive (blob);
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
			get {
				var r = (int)HarfBuzzApi.hb_face_get_index (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_index (Handle, (uint)value);
			}
		}

		public int UnitsPerEm {
			get {
				var r = (int)HarfBuzzApi.hb_face_get_upem (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_upem (Handle, (uint)value);
			}
		}

		public int GlyphCount {
			get {
				var r = (int)HarfBuzzApi.hb_face_get_glyph_count (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_glyph_count (Handle, (uint)value);
			}
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

		public Blob ReferenceTable (Tag table)
		{
			var r = new Blob (HarfBuzzApi.hb_face_reference_table (Handle, table));
			return r;
		}

		public bool IsImmutable {
			get {
				var r = HarfBuzzApi.hb_face_is_immutable (Handle);
				return r;
			}
		}

		public void MakeImmutable ()
		{
			HarfBuzzApi.hb_face_make_immutable (Handle);
		}

		// Variable font support

		public bool HasVariationData {
			get {
				var r = HarfBuzzApi.hb_ot_var_has_data (Handle);
				return r;
			}
		}

		public int VariationAxisCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_var_get_axis_count (Handle);
				return r;
			}
		}

		public OpenTypeVarAxisInfo[] VariationAxisInfos
		{
			get {
				var count = HarfBuzzApi.hb_ot_var_get_axis_count (Handle);
				if (count == 0) {
					return Array.Empty<OpenTypeVarAxisInfo> ();
				}

				var axes = new OpenTypeVarAxisInfo[(int)count];
				fixed (OpenTypeVarAxisInfo* ptr = axes) {
					HarfBuzzApi.hb_ot_var_get_axis_infos (Handle, 0, &count, ptr);
				}
				return axes;
			}
		}

		public int GetVariationAxisInfos (Span<OpenTypeVarAxisInfo> axes)
		{
			uint count = (uint)axes.Length;
			fixed (OpenTypeVarAxisInfo* ptr = axes) {
				HarfBuzzApi.hb_ot_var_get_axis_infos (Handle, 0, &count, ptr);
			}
			return (int)count;
		}

		public bool TryFindVariationAxis (Tag tag, out OpenTypeVarAxisInfo axisInfo)
		{
			axisInfo = default;
			fixed (OpenTypeVarAxisInfo* ptr = &axisInfo) {
				var r = HarfBuzzApi.hb_ot_var_find_axis_info (Handle, tag, ptr);
				return r;
			}
		}

		public int NamedInstanceCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_var_get_named_instance_count (Handle);
				return r;
			}
		}

		public OpenTypeNameId GetNamedInstanceSubfamilyNameId (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));
			var r = HarfBuzzApi.hb_ot_var_named_instance_get_subfamily_name_id (Handle, (uint)instanceIndex);
			return r;
		}

		public OpenTypeNameId GetNamedInstancePostScriptNameId (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));
			var r = HarfBuzzApi.hb_ot_var_named_instance_get_postscript_name_id (Handle, (uint)instanceIndex);
			return r;
		}

		public int GetNamedInstanceDesignCoordsCount (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			// Return value is the total number of design coordinates
			var r = (int)HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, null, null);
			return r;
		}

		public float[] GetNamedInstanceDesignCoords (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			// Return value is the total number of design coordinates
			var totalCoords = (int)HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, null, null);
			if (totalCoords == 0) {
				return Array.Empty<float> ();
			}

			uint coordsLength = (uint)totalCoords;
			var coords = new float[totalCoords];
			fixed (float* ptr = coords) {
				HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, &coordsLength, ptr);
			}
			return coords;
		}

		public int GetNamedInstanceDesignCoords (int instanceIndex, Span<float> coords)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			uint coordsLength = (uint)coords.Length;
			fixed (float* ptr = coords) {
				HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, &coordsLength, ptr);
			}
			return (int)coordsLength;
		}

		// Color font / palette support

		public bool HasPalettes {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_palettes (Handle);
				return r;
			}
		}

		public int PaletteCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_color_palette_get_count (Handle);
				return r;
			}
		}

		public HBColor[] GetPaletteColors (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));

			var totalColors = (int)HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, null, null);
			if (totalColors == 0) {
				return Array.Empty<HBColor> ();
			}

			uint count = (uint)totalColors;
			var colors = new HBColor[totalColors];
			fixed (HBColor* ptr = colors) {
				HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, &count, ptr);
			}
			return colors;
		}

		public int GetPaletteColors (int paletteIndex, Span<HBColor> colors)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));

			uint count = (uint)colors.Length;
			fixed (HBColor* ptr = colors) {
				HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, &count, ptr);
			}
			return (int)count;
		}

		public OpenTypeColorPaletteFlags GetPaletteFlags (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_get_flags (Handle, (uint)paletteIndex);
			return r;
		}

		public OpenTypeNameId GetPaletteNameId (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_get_name_id (Handle, (uint)paletteIndex);
			return r;
		}

		public OpenTypeNameId GetPaletteColorNameId (int colorIndex)
		{
			if (colorIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (colorIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_color_get_name_id (Handle, (uint)colorIndex);
			return r;
		}

		public bool HasColorLayers {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_layers (Handle);
				return r;
			}
		}

		public bool HasColorPng {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_png (Handle);
				return r;
			}
		}

		public bool HasColorSvg {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_svg (Handle);
				return r;
			}
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
