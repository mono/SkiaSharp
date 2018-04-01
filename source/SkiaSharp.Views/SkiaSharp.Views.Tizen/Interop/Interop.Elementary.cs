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
	internal static partial class Elementary
	{
		[DllImport(Libraries.Elementary)]
		internal static extern IntPtr elm_layout_add(IntPtr obj);

		[DllImport(Libraries.Elementary)]
		internal static extern bool elm_layout_theme_set(IntPtr obj, string klass, string group, string style);

		[DllImport(Libraries.Elementary)]
		internal static extern IntPtr elm_object_part_content_get(IntPtr obj, string part);

		[DllImport(Libraries.Elementary)]
		internal static extern void elm_object_part_content_set(IntPtr obj, string part, IntPtr content);
	}
}
