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

internal static partial class Interop
{
	private static class Libraries
	{
		internal const string Libc = "libc.so.6";
		internal const string Evas = "libevas.so.1";
		internal const string Elementary = "libelementary.so.1";
		internal const string Eina = "libeina.so.1";
		internal const string Ecore = "libecore.so.1";
		internal const string Eo = "libeo.so.1";
		internal const string Eext = "libefl-extension.so.0";
	}
}
