using System;
using System.IO;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SkiaSharp.Tests
{
	public class SKApiTest : SKTest
	{
		private static IEnumerable<MethodInfo> GetApi()
		{
			var ass = typeof(SKImageInfo).GetTypeInfo().Assembly;
			var api = ass.GetType("SkiaSharp.SkiaApi").GetMethods().Where(a => a.GetCustomAttribute<DllImportAttribute>() != null);
			return api;
		}

		[Test]
		public void ApiTypesAreMarshalledCorrectly()
		{
			var api = GetApi();

			foreach (var method in api)
			{
				foreach (var param in method.GetParameters())
				{
					var paramType = param.ParameterType;

					if (paramType == typeof(bool))
					{
						//check string
						var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
						Assert.NotNull(marshal, $"{method.Name}({paramType})");
						Assert.AreEqual(UnmanagedType.I1, marshal.Value, $"{method.Name}({paramType})");
					}
					if (paramType == typeof(string))
					{
						//check string
						var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
						Assert.NotNull(marshal, $"{method.Name}({paramType})");
						Assert.AreEqual(UnmanagedType.LPStr, marshal.Value, $"{method.Name}({paramType})");
					}
					else if (paramType == typeof(string[]))
					{
						// check array of strings
						var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
						Assert.NotNull(marshal, $"{method.Name}({paramType})");
						Assert.AreEqual(UnmanagedType.LPArray, marshal.Value, $"{method.Name}({paramType})");
						Assert.AreEqual(UnmanagedType.LPStr, marshal.ArraySubType, $"{method.Name}({paramType})");
					}
					else
					{
						if (param.ParameterType.IsByRef || param.ParameterType.IsArray)
						{
							paramType = param.ParameterType.GetElementType();
						}

						// make sure only structs
						Assert.False(paramType.GetTypeInfo().IsClass, $"{method.Name}({paramType})");

						// make sure our structs have a layout type
						if (!paramType.GetTypeInfo().IsEnum && paramType.Namespace == "SkiaSharp")
						{
							// check blittable
							try
							{
								GCHandle.Alloc(Activator.CreateInstance(paramType), GCHandleType.Pinned).Free();
							}
							catch
							{
								Assert.Fail($"not blittable : {method.Name}({paramType})");
							}
						}
					}
				}

				if (method.ReturnParameter.ParameterType == typeof(bool))
				{
					var marshal = method.ReturnParameter.GetCustomAttribute<MarshalAsAttribute>();
					Assert.NotNull(marshal, $"{method.Name}(return)");
					Assert.AreEqual(UnmanagedType.I1, marshal.Value, $"{method.Name}(return)");
				}
			}
		}
	}
}
