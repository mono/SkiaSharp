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
using System.Reflection;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Evas
	{
		internal static partial class GL
		{
			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_gl_new(IntPtr evas);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_gl_free(IntPtr evas_gl);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_gl_context_create(IntPtr evas_gl, IntPtr share_ctx);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_gl_context_destroy(IntPtr evas_gl, IntPtr ctx);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_gl_surface_create(IntPtr evas_gl, IntPtr config, int width, int height);

			[DllImport(Libraries.Evas)]
			internal static extern void evas_gl_surface_destroy(IntPtr evas_gl, IntPtr surf);

			[DllImport(Libraries.Evas)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool evas_gl_native_surface_get(IntPtr evas_gl, IntPtr surf, out NativeSurfaceOpenGL ns);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_gl_proc_address_get(IntPtr evas_gl, string name);

			[DllImport(Libraries.Evas)]
			internal static extern IntPtr evas_gl_api_get(IntPtr evas_gl);

			[DllImport(Libraries.Evas)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool evas_gl_make_current(IntPtr evas_gl, IntPtr surf, IntPtr ctx);

			internal struct Config
			{
				/// <summary>
				/// Surface Color Format
				/// </summary>
				public ColorFormat color_format;
				/// <summary>
				/// Surface Depth Bits
				/// </summary>
				public DepthBits depth_bits;
				/// <summary>
				/// Surface Stencil Bits
				/// </summary>
				public StencilBits stencil_bits;
				/// <summary>
				/// Extra Surface Options
				/// </summary>
				public OptionsBits options_bits;
				/// <summary>
				/// Optional Surface MSAA Bits
				/// </summary>
				public MultisampleBits multisample_bits;
#pragma warning disable 0169
				/// <summary>
				/// @internal Special flag for OpenGL-ES 1.1 indirect rendering surfaces
				/// </summary>
				/// <remarks>
				/// Not used.
				/// </remarks>
				private ContextVersion gles_version;
#pragma warning restore 0169
			}

			internal struct NativeSurfaceOpenGL
			{
				// This structure is used to move data from one entity into another.
#pragma warning disable 0169
				/// <summary>
				/// OpenGL texture id to use from glGenTextures().
				/// </summary>
				uint texture_id;
				/// <summary>
				/// 0 if not a FBO, FBO id otherwise from glGenFramebuffers().
				/// </summary>
				uint framebuffer_id;
				/// <summary>
				/// Same as 'internalFormat' for glTexImage2D().
				/// </summary>
				uint internal_format;
				/// <summary>
				/// Same as 'format' for glTexImage2D().
				/// </summary>
				uint format;
				/// <summary>
				/// Region inside the texture to use (image size is assumed as texture size, 
				/// with 0, 0 being the top-left and co-ordinates working down to the right and bottom being positive).
				/// </summary>
				uint x;
				uint y;
				uint w;
				uint h;
#pragma warning restore 0169
			}

			internal enum ColorFormat
			{
				/// <summary>
				/// Opaque RGB surface
				/// </summary>
				RGB_888 = 0,
				/// <summary>
				/// RGBA surface with alpha
				/// </summary>
				RGBA_8888 = 1,
				/// <summary>
				/// Special value for creating PBuffer surfaces without any attached buffer.
				/// </summary>
				NO_FBO = 2
			}

			internal enum DepthBits
			{
				NONE = 0,
				/// <summary>
				/// 8 bits precision surface depth
				/// </summary>
				BIT_8 = 1,
				/// <summary>
				/// 16 bits precision surface depth
				/// </summary>
				BIT_16 = 2,
				/// <summary>
				/// 24 bits precision surface depth
				/// </summary>
				BIT_24 = 3,
				/// <summary>
				/// 32 bits precision surface depth
				/// </summary>
				BIT_32 = 4
			}

			internal enum StencilBits
			{
				NONE = 0,
				/// <summary>
				/// 1 bit precision for stencil buffer
				/// </summary>
				BIT_1 = 1,
				/// <summary>
				/// 2 bits precision for stencil buffer
				/// </summary>
				BIT_2 = 2,
				/// <summary>
				/// 4 bits precision for stencil buffer
				/// </summary>
				BIT_4 = 3,
				/// <summary>
				/// 8 bits precision for stencil buffer
				/// </summary>
				BIT_8 = 4,
				/// <summary>
				/// 16 bits precision for stencil buffer
				/// </summary>
				BIT_16 = 5
			}

			internal enum OptionsBits
			{
				/// <summary>
				/// No extra options.
				/// </summary>
				NONE = 0,
				/// <summary>
				/// Optional hint to allow rendering directly to the Evas window if possible.
				/// </summary>
				DIRECT = (1 << 0),
				/// <summary>
				/// Force direct rendering even if the canvas is rotated.
				/// </summary>
				CLIENT_SIDE_ROTATION = (1 << 1),
				/// <summary>
				/// If enabled, Evas GL pixel callback will be called by another thread instead of main thread.
				/// </summary>
				THREAD = (1 << 2)
			}

			internal enum MultisampleBits
			{
				/// <summary>
				/// No multisample rendering.
				/// </summary>
				NONE = 0,
				/// <summary>
				/// MSAA with minimum number of samples.
				/// </summary>
				LOW = 1,
				/// <summary>
				/// MSAA with half the maximum number of samples.
				/// </summary>
				MED = 2,
				/// <summary>
				/// MSAA with maximum allowed samples.
				/// </summary>
				HIGH = 3
			}

			internal enum ContextVersion
			{
				/// <summary>
				/// OpenGL-ES 1.x
				/// </summary>
				GLES_1_X = 1,
				/// <summary>
				/// OpenGL-ES 2.x is the default
				/// </summary>
				GLES_2_X = 2,
				/// <summary>
				/// OpenGL-ES 3.x (@b Since 2.4)
				/// </summary>
				GLES_3_X = 3,
				/// <summary>
				/// Enable debug mode on this context (See GL_KHR_debug) (@b Since 4.0)
				/// </summary>
				DEBUG = 0x1000
			}
		}
	}
}
