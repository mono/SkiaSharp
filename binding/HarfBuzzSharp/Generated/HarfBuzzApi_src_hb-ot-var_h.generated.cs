using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-var.h

		// extern hb_bool_t hb_ot_var_find_axis_info(hb_face_t* face, hb_tag_t axis_tag, hb_ot_var_axis_info_t* axis_info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_var_find_axis_info (IntPtr face, UInt32 axis_tag, OpenTypeVarAxisInfo* axis_info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_var_find_axis_info (IntPtr face, UInt32 axis_tag, OpenTypeVarAxisInfo* axis_info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_var_find_axis_info (IntPtr face, UInt32 axis_tag, OpenTypeVarAxisInfo* axis_info);
		}
		private static Delegates.hb_ot_var_find_axis_info hb_ot_var_find_axis_info_delegate;
		internal static bool hb_ot_var_find_axis_info (IntPtr face, UInt32 axis_tag, OpenTypeVarAxisInfo* axis_info) =>
			(hb_ot_var_find_axis_info_delegate ??= GetSymbol<Delegates.hb_ot_var_find_axis_info> ("hb_ot_var_find_axis_info")).Invoke (face, axis_tag, axis_info);
		#endif

		// extern unsigned int hb_ot_var_get_axis_count(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_var_get_axis_count (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_var_get_axis_count (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_var_get_axis_count (IntPtr face);
		}
		private static Delegates.hb_ot_var_get_axis_count hb_ot_var_get_axis_count_delegate;
		internal static UInt32 hb_ot_var_get_axis_count (IntPtr face) =>
			(hb_ot_var_get_axis_count_delegate ??= GetSymbol<Delegates.hb_ot_var_get_axis_count> ("hb_ot_var_get_axis_count")).Invoke (face);
		#endif

		// extern unsigned int hb_ot_var_get_axis_infos(hb_face_t* face, unsigned int start_offset, unsigned int* axes_count, hb_ot_var_axis_info_t* axes_array)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_var_get_axis_infos (IntPtr face, UInt32 start_offset, UInt32* axes_count, OpenTypeVarAxisInfo* axes_array);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_var_get_axis_infos (IntPtr face, UInt32 start_offset, UInt32* axes_count, OpenTypeVarAxisInfo* axes_array);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_var_get_axis_infos (IntPtr face, UInt32 start_offset, UInt32* axes_count, OpenTypeVarAxisInfo* axes_array);
		}
		private static Delegates.hb_ot_var_get_axis_infos hb_ot_var_get_axis_infos_delegate;
		internal static UInt32 hb_ot_var_get_axis_infos (IntPtr face, UInt32 start_offset, UInt32* axes_count, OpenTypeVarAxisInfo* axes_array) =>
			(hb_ot_var_get_axis_infos_delegate ??= GetSymbol<Delegates.hb_ot_var_get_axis_infos> ("hb_ot_var_get_axis_infos")).Invoke (face, start_offset, axes_count, axes_array);
		#endif

		// extern unsigned int hb_ot_var_get_named_instance_count(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_var_get_named_instance_count (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_var_get_named_instance_count (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_var_get_named_instance_count (IntPtr face);
		}
		private static Delegates.hb_ot_var_get_named_instance_count hb_ot_var_get_named_instance_count_delegate;
		internal static UInt32 hb_ot_var_get_named_instance_count (IntPtr face) =>
			(hb_ot_var_get_named_instance_count_delegate ??= GetSymbol<Delegates.hb_ot_var_get_named_instance_count> ("hb_ot_var_get_named_instance_count")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_var_has_data(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_var_has_data (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_var_has_data (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_var_has_data (IntPtr face);
		}
		private static Delegates.hb_ot_var_has_data hb_ot_var_has_data_delegate;
		internal static bool hb_ot_var_has_data (IntPtr face) =>
			(hb_ot_var_has_data_delegate ??= GetSymbol<Delegates.hb_ot_var_has_data> ("hb_ot_var_has_data")).Invoke (face);
		#endif

		// extern unsigned int hb_ot_var_named_instance_get_design_coords(hb_face_t* face, unsigned int instance_index, unsigned int* coords_length, float* coords)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_var_named_instance_get_design_coords (IntPtr face, UInt32 instance_index, UInt32* coords_length, Single* coords);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_var_named_instance_get_design_coords (IntPtr face, UInt32 instance_index, UInt32* coords_length, Single* coords);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_var_named_instance_get_design_coords (IntPtr face, UInt32 instance_index, UInt32* coords_length, Single* coords);
		}
		private static Delegates.hb_ot_var_named_instance_get_design_coords hb_ot_var_named_instance_get_design_coords_delegate;
		internal static UInt32 hb_ot_var_named_instance_get_design_coords (IntPtr face, UInt32 instance_index, UInt32* coords_length, Single* coords) =>
			(hb_ot_var_named_instance_get_design_coords_delegate ??= GetSymbol<Delegates.hb_ot_var_named_instance_get_design_coords> ("hb_ot_var_named_instance_get_design_coords")).Invoke (face, instance_index, coords_length, coords);
		#endif

		// extern hb_ot_name_id_t hb_ot_var_named_instance_get_postscript_name_id(hb_face_t* face, unsigned int instance_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeNameId hb_ot_var_named_instance_get_postscript_name_id (IntPtr face, UInt32 instance_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_var_named_instance_get_postscript_name_id (IntPtr face, UInt32 instance_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameId hb_ot_var_named_instance_get_postscript_name_id (IntPtr face, UInt32 instance_index);
		}
		private static Delegates.hb_ot_var_named_instance_get_postscript_name_id hb_ot_var_named_instance_get_postscript_name_id_delegate;
		internal static OpenTypeNameId hb_ot_var_named_instance_get_postscript_name_id (IntPtr face, UInt32 instance_index) =>
			(hb_ot_var_named_instance_get_postscript_name_id_delegate ??= GetSymbol<Delegates.hb_ot_var_named_instance_get_postscript_name_id> ("hb_ot_var_named_instance_get_postscript_name_id")).Invoke (face, instance_index);
		#endif

		// extern hb_ot_name_id_t hb_ot_var_named_instance_get_subfamily_name_id(hb_face_t* face, unsigned int instance_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeNameId hb_ot_var_named_instance_get_subfamily_name_id (IntPtr face, UInt32 instance_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_var_named_instance_get_subfamily_name_id (IntPtr face, UInt32 instance_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameId hb_ot_var_named_instance_get_subfamily_name_id (IntPtr face, UInt32 instance_index);
		}
		private static Delegates.hb_ot_var_named_instance_get_subfamily_name_id hb_ot_var_named_instance_get_subfamily_name_id_delegate;
		internal static OpenTypeNameId hb_ot_var_named_instance_get_subfamily_name_id (IntPtr face, UInt32 instance_index) =>
			(hb_ot_var_named_instance_get_subfamily_name_id_delegate ??= GetSymbol<Delegates.hb_ot_var_named_instance_get_subfamily_name_id> ("hb_ot_var_named_instance_get_subfamily_name_id")).Invoke (face, instance_index);
		#endif

		// extern void hb_ot_var_normalize_coords(hb_face_t* face, unsigned int coords_length, const float* design_coords, int* normalized_coords)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_var_normalize_coords (IntPtr face, UInt32 coords_length, Single* design_coords, Int32* normalized_coords);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_var_normalize_coords (IntPtr face, UInt32 coords_length, Single* design_coords, Int32* normalized_coords);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_var_normalize_coords (IntPtr face, UInt32 coords_length, Single* design_coords, Int32* normalized_coords);
		}
		private static Delegates.hb_ot_var_normalize_coords hb_ot_var_normalize_coords_delegate;
		internal static void hb_ot_var_normalize_coords (IntPtr face, UInt32 coords_length, Single* design_coords, Int32* normalized_coords) =>
			(hb_ot_var_normalize_coords_delegate ??= GetSymbol<Delegates.hb_ot_var_normalize_coords> ("hb_ot_var_normalize_coords")).Invoke (face, coords_length, design_coords, normalized_coords);
		#endif

		// extern void hb_ot_var_normalize_variations(hb_face_t* face, const hb_variation_t* variations, unsigned int variations_length, int* coords, unsigned int coords_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_var_normalize_variations (IntPtr face, Variation* variations, UInt32 variations_length, Int32* coords, UInt32 coords_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_var_normalize_variations (IntPtr face, Variation* variations, UInt32 variations_length, Int32* coords, UInt32 coords_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_var_normalize_variations (IntPtr face, Variation* variations, UInt32 variations_length, Int32* coords, UInt32 coords_length);
		}
		private static Delegates.hb_ot_var_normalize_variations hb_ot_var_normalize_variations_delegate;
		internal static void hb_ot_var_normalize_variations (IntPtr face, Variation* variations, UInt32 variations_length, Int32* coords, UInt32 coords_length) =>
			(hb_ot_var_normalize_variations_delegate ??= GetSymbol<Delegates.hb_ot_var_normalize_variations> ("hb_ot_var_normalize_variations")).Invoke (face, variations, variations_length, coords, coords_length);
		#endif

		#endregion

	}
}
