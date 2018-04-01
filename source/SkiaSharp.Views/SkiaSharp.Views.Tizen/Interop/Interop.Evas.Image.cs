/*
 * Copyright (c) 2016 Samsung Electronics Co., Ltd All Rights Reserved
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Evas
	{
		internal static partial class Image
		{
			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_object_image_add(IntPtr obj);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_object_image_filled_add(IntPtr obj);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_size_get(IntPtr obj, IntPtr x, out int y);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_size_get(IntPtr obj, out int x, IntPtr y);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_size_get(IntPtr obj, out int x, out int y);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_size_set(IntPtr obj, int w, int h);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_object_image_data_get(IntPtr obj, bool for_writing);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_data_set(IntPtr obj, IntPtr data);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_data_update_add(IntPtr obj, int x, int y, int w, int h);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_colorspace_set(IntPtr obj, Colorspace cspace);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_fill_set(IntPtr obj, int x, int y, int w, int h);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_native_surface_set(IntPtr obj, ref GL.NativeSurfaceOpenGL surf);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_native_surface_set(IntPtr obj, IntPtr zero);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_pixels_dirty_set(IntPtr obj, bool dirty);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_pixels_get_callback_set(IntPtr obj, ImagePixelsSetCallback func, IntPtr data);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_pixels_get_callback_set(IntPtr obj, IntPtr zero, IntPtr data);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_smooth_scale_set(IntPtr obj, bool smooth_scale);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_object_image_alpha_set(IntPtr obj, bool has_alpha);

			public delegate void ImagePixelsSetCallback(IntPtr data, IntPtr o);

			internal enum Colorspace
			{
				ARGB8888,
				YCBCR422P601_PL,
				YCBCR422P709_PL,
				RGB565_A5P,
				GRY8 = 4,
				YCBCR422601_PL,
				YCBCR420NV12601_PL,
				YCBCR420TM12601_PL,
				AGRY88 = 8,
				ETC1 = 9,
				RGB8_ETC2 = 10,
				RGBA8_ETC2_EAC = 11,
				ETC1_ALPHA = 12,
				RGB_S3TC_DXT1 = 13,
				RGBA_S3TC_DXT1 = 14,
				RGBA_S3TC_DXT2 = 15,
				RGBA_S3TC_DXT3 = 16,
				RGBA_S3TC_DXT4 = 17,
				RGBA_S3TC_DXT5 = 18,
			}
		}
	}
}
