#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Represents a typeface.</summary>
	/// <remarks />
	public unsafe class Face : NativeObject
	{
		private static readonly Lazy<Face> emptyFace = new Lazy<Face> (() => new StaticFace (HarfBuzzApi.hb_face_get_empty ()));

		/// <summary>Gets a reference to the empty <see cref="T:HarfBuzzSharp.Face" /> instance.</summary>
		/// <value>The empty <see cref="T:HarfBuzzSharp.Face" /> instance.</value>
		/// <remarks />
		public static Face Empty => emptyFace.Value;

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Face" /> class, using the specified typeface blob.</summary>
		/// <param name="blob">The typeface data.</param>
		/// <param name="index">The zero-based face index in a collection.</param>
		/// <remarks />
		public Face (Blob blob, uint index)
			: this (blob, (int)index)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Face" /> class, using the specified typeface blob.</summary>
		/// <param name="blob">The typeface data.</param>
		/// <param name="index">The zero-based face index in a collection.</param>
		/// <remarks />
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

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Face" /> class, using the delegate to assemble the data.</summary>
		/// <param name="getTable">The delegate to retrieve the table data.</param>
		/// <remarks />
		public Face (GetTableDelegate getTable)
			: this (getTable, null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Face" /> class, using the delegate to assemble the data.</summary>
		/// <param name="getTable">The delegate to retrieve the table data.</param>
		/// <param name="destroy">The delegate to call when the face is destroyed, or <see langword="null" />.</param>
		/// <remarks />
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

		/// <summary>Gets or sets the zero-based face index in a collection.</summary>
		/// <value>The zero-based face index in a collection.</value>
		/// <remarks />
		public int Index {
			get {
				var r = (int)HarfBuzzApi.hb_face_get_index (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_index (Handle, (uint)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the units per EM.</summary>
		/// <value>The units per EM.</value>
		/// <remarks />
		public int UnitsPerEm {
			get {
				var r = (int)HarfBuzzApi.hb_face_get_upem (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_upem (Handle, (uint)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the number of glyphs in the face.</summary>
		/// <value>The number of glyphs in the face.</value>
		/// <remarks />
		public int GlyphCount {
			get {
				var r = (int)HarfBuzzApi.hb_face_get_glyph_count (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_face_set_glyph_count (Handle, (uint)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets the list of OpenType table tags present in this face.</summary>
		/// <value>An array of <see cref="T:HarfBuzzSharp.Tag" /> values representing the tables in the face.</value>
		/// <remarks />
		public unsafe Tag[] Tables {
			get {
				uint tableCount;
				var count = HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &tableCount, null);
				var buffer = new Tag[count];
				fixed (void* ptr = buffer) {
					HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &count, (uint*)ptr);
				}
				GC.KeepAlive (this);
				return buffer;
			}
		}

		/// <summary>Retrieves a reference to the specified font table as a blob.</summary>
		/// <param name="table">The tag identifying the table to retrieve.</param>
		/// <returns>A <see cref="T:HarfBuzzSharp.Blob" /> containing the table data, or an empty blob if the table is not found.</returns>
		/// <remarks />
		public Blob ReferenceTable (Tag table)
		{
			var r = new Blob (HarfBuzzApi.hb_face_reference_table (Handle, table));
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets a value indicating whether this face is immutable.</summary>
		/// <value><see langword="true" /> if the face is immutable; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsImmutable {
			get {
				var r = HarfBuzzApi.hb_face_is_immutable (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Makes this face immutable, preventing further modifications.</summary>
		/// <remarks />
		public void MakeImmutable ()
		{
			HarfBuzzApi.hb_face_make_immutable (Handle);
			GC.KeepAlive (this);
		}

		// Variable font support

		/// <summary>Gets a value indicating whether the font face contains OpenType variation data.</summary>
		/// <value><see langword="true" /> if the face contains OpenType variation data (fvar table); otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasVariationData {
			get {
				var r = HarfBuzzApi.hb_ot_var_has_data (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the number of variation axes in the font face.</summary>
		/// <value>The number of variation axes defined in the font's fvar table.</value>
		/// <remarks />
		public int VariationAxisCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_var_get_axis_count (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets information about all variation axes in the font face.</summary>
		/// <value>An array of <see cref="T:HarfBuzzSharp.OpenTypeVarAxisInfo" /> describing each variation axis.</value>
		/// <remarks />
		public OpenTypeVarAxisInfo[] VariationAxisInfos
		{
			get {
				var count = HarfBuzzApi.hb_ot_var_get_axis_count (Handle);
				if (count == 0) {
					GC.KeepAlive (this);
					return Array.Empty<OpenTypeVarAxisInfo> ();
				}

				var axes = new OpenTypeVarAxisInfo[(int)count];
				fixed (OpenTypeVarAxisInfo* ptr = axes) {
					HarfBuzzApi.hb_ot_var_get_axis_infos (Handle, 0, &count, ptr);
				}
				GC.KeepAlive (this);
				return axes;
			}
		}

		/// <summary>Fills a span with information about the font's variation axes.</summary>
		/// <param name="axes">A span to receive the <see cref="T:HarfBuzzSharp.OpenTypeVarAxisInfo" /> values.</param>
		/// <returns>The number of variation axis infos written to <paramref name="axes" />.</returns>
		/// <remarks />
		public int GetVariationAxisInfos (Span<OpenTypeVarAxisInfo> axes)
		{
			uint count = (uint)axes.Length;
			fixed (OpenTypeVarAxisInfo* ptr = axes) {
				HarfBuzzApi.hb_ot_var_get_axis_infos (Handle, 0, &count, ptr);
			}
			GC.KeepAlive (this);
			return (int)count;
		}

		/// <summary>Attempts to find information about the variation axis with the specified tag.</summary>
		/// <param name="tag">The four-byte OpenType tag of the variation axis to find.</param>
		/// <param name="axisInfo">When this method returns, contains the axis information if found. This parameter is treated as uninitialized.</param>
		/// <returns><see langword="true" /> if the axis was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryFindVariationAxis (Tag tag, out OpenTypeVarAxisInfo axisInfo)
		{
			axisInfo = default;
			fixed (OpenTypeVarAxisInfo* ptr = &axisInfo) {
				var r = HarfBuzzApi.hb_ot_var_find_axis_info (Handle, tag, ptr);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the number of named instances in the font face.</summary>
		/// <value>The number of named instances defined in the font's fvar table.</value>
		/// <remarks />
		public int NamedInstanceCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_var_get_named_instance_count (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Returns the OpenType name ID of the subfamily name for the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.OpenTypeNameId" /> of the subfamily name string for the named instance.</returns>
		/// <remarks />
		public OpenTypeNameId GetNamedInstanceSubfamilyNameId (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));
			var r = HarfBuzzApi.hb_ot_var_named_instance_get_subfamily_name_id (Handle, (uint)instanceIndex);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns the OpenType name ID of the PostScript name for the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.OpenTypeNameId" /> of the PostScript name string for the named instance.</returns>
		/// <remarks />
		public OpenTypeNameId GetNamedInstancePostScriptNameId (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));
			var r = HarfBuzzApi.hb_ot_var_named_instance_get_postscript_name_id (Handle, (uint)instanceIndex);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns the number of design-space coordinates for the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance.</param>
		/// <returns>The number of design-space coordinates for the named instance.</returns>
		/// <remarks />
		public int GetNamedInstanceDesignCoordsCount (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			// Return value is the total number of design coordinates
			var r = (int)HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, null, null);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns the design-space coordinates for the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance.</param>
		/// <returns>An array of design-space coordinate values for the named instance, one per variation axis.</returns>
		/// <remarks />
		public float[] GetNamedInstanceDesignCoords (int instanceIndex)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			// Return value is the total number of design coordinates
			var totalCoords = (int)HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, null, null);
			if (totalCoords == 0) {
				GC.KeepAlive (this);
				return Array.Empty<float> ();
			}

			uint coordsLength = (uint)totalCoords;
			var coords = new float[totalCoords];
			fixed (float* ptr = coords) {
				HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, &coordsLength, ptr);
			}
			GC.KeepAlive (this);
			return coords;
		}

		/// <summary>Fills a span with the design-space coordinates for the specified named instance.</summary>
		/// <param name="instanceIndex">The zero-based index of the named instance.</param>
		/// <param name="coords">A span to receive the design-space coordinate values.</param>
		/// <returns>The number of design-space coordinates written to <paramref name="coords" />.</returns>
		/// <remarks />
		public int GetNamedInstanceDesignCoords (int instanceIndex, Span<float> coords)
		{
			if (instanceIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (instanceIndex));

			uint coordsLength = (uint)coords.Length;
			fixed (float* ptr = coords) {
				HarfBuzzApi.hb_ot_var_named_instance_get_design_coords (Handle, (uint)instanceIndex, &coordsLength, ptr);
			}
			GC.KeepAlive (this);
			return (int)coordsLength;
		}

		// Color font / palette support

		/// <summary>Gets a value indicating whether the font face contains a CPAL color-palette table.</summary>
		/// <value><see langword="true" /> if the face contains a CPAL color-palette table; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasPalettes {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_palettes (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the number of color palettes in the font face.</summary>
		/// <value>The number of color palettes defined in the font's CPAL table.</value>
		/// <remarks />
		public int PaletteCount {
			get {
				var r = (int)HarfBuzzApi.hb_ot_color_palette_get_count (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Returns all color entries for the specified font palette.</summary>
		/// <param name="paletteIndex">The zero-based index of the palette.</param>
		/// <returns>An array of <see cref="T:HarfBuzzSharp.HBColor" /> values for the palette, in BGRA byte order.</returns>
		/// <remarks />
		public HBColor[] GetPaletteColors (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));

			var totalColors = (int)HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, null, null);
			if (totalColors == 0) {
				GC.KeepAlive (this);
				return Array.Empty<HBColor> ();
			}

			uint count = (uint)totalColors;
			var colors = new HBColor[totalColors];
			fixed (HBColor* ptr = colors) {
				HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, &count, ptr);
			}
			GC.KeepAlive (this);
			return colors;
		}

		/// <summary>Fills a span with the color entries for the specified font palette.</summary>
		/// <param name="paletteIndex">The zero-based index of the palette.</param>
		/// <param name="colors">A span to receive the <see cref="T:HarfBuzzSharp.HBColor" /> values.</param>
		/// <returns>The number of color entries written to <paramref name="colors" />.</returns>
		/// <remarks />
		public int GetPaletteColors (int paletteIndex, Span<HBColor> colors)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));

			uint count = (uint)colors.Length;
			fixed (HBColor* ptr = colors) {
				HarfBuzzApi.hb_ot_color_palette_get_colors (Handle, (uint)paletteIndex, 0, &count, ptr);
			}
			GC.KeepAlive (this);
			return (int)count;
		}

		/// <summary>Returns the flags for the specified font palette.</summary>
		/// <param name="paletteIndex">The zero-based index of the palette.</param>
		/// <returns>A bitwise combination of <see cref="T:HarfBuzzSharp.OpenTypeColorPaletteFlags" /> values describing the palette.</returns>
		/// <remarks />
		public OpenTypeColorPaletteFlags GetPaletteFlags (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_get_flags (Handle, (uint)paletteIndex);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns the OpenType name ID of the name string for the specified palette.</summary>
		/// <param name="paletteIndex">The zero-based index of the palette.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.OpenTypeNameId" /> of the palette name string.</returns>
		/// <remarks />
		public OpenTypeNameId GetPaletteNameId (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_get_name_id (Handle, (uint)paletteIndex);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns the OpenType name ID of the name string for the specified palette color entry.</summary>
		/// <param name="colorIndex">The zero-based index of the color entry within the palette.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.OpenTypeNameId" /> of the color-entry name string.</returns>
		/// <remarks />
		public OpenTypeNameId GetPaletteColorNameId (int colorIndex)
		{
			if (colorIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (colorIndex));
			var r = HarfBuzzApi.hb_ot_color_palette_color_get_name_id (Handle, (uint)colorIndex);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Gets a value indicating whether the font face contains a COLR color-layers table.</summary>
		/// <value><see langword="true" /> if the face contains a COLR color-layers table; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasColorLayers {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_layers (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating whether the font face contains a PNG color-bitmap table.</summary>
		/// <value><see langword="true" /> if the face contains a CBDT/CBLC PNG color-bitmap table; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasColorPng {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_png (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating whether the font face contains an SVG color table.</summary>
		/// <value><see langword="true" /> if the face contains an SVG color table; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasColorSvg {
			get {
				var r = HarfBuzzApi.hb_ot_color_has_svg (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}


		/// <summary>Releases the unmanaged resources used by the <see cref="T:HarfBuzzSharp.Face" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:HarfBuzzSharp.Face" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the unmanaged resources used.</summary>
		/// <remarks />
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
