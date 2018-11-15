using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Xunit;
using Xunit.Sdk;
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

		[SkippableFact]
		public void ApiTypesAreNotInvalid()
		{
			var ass = typeof(SKImageInfo).GetTypeInfo().Assembly;

			var api = GetApi();

			foreach (var method in api)
			{
				foreach (var param in method.GetParameters())
				{
					var paramType = param.ParameterType;
					if (param.ParameterType.IsByRef || param.ParameterType.IsArray)
					{
						paramType = param.ParameterType.GetElementType();
					}

					// check to make sure that the "internal" versions are being used
					var internalType = ass.GetType(paramType.FullName + "Internal");
					var nativeType = ass.GetType(paramType.FullName + "Native");
					if (internalType != null || nativeType != null)
					{
						throw new XunitException($"{method.Name}: Using type {paramType.FullName}, but type {(internalType ?? nativeType).FullName} exists.");
					}
				}
			}
		}

		[SkippableFact]
		public void ApiReturnTypesArePrimitives()
		{
			var api = GetApi();

			foreach (var method in api)
			{
				var prim = method.ReturnType.GetTypeInfo().IsPrimitive;
				var enm = method.ReturnType.GetTypeInfo().IsEnum;
				var voidType = method.ReturnType == typeof(void);
				Assert.True(prim || enm || voidType, method.Name);
			}
		}

		[SkippableFact]
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
						if (marshal == null)
							throw new XunitException($"{method.Name}({paramType})");
						if (marshal.Value != UnmanagedType.I1)
							throw new XunitException($"{method.Name}({paramType})");
					}
					if (paramType == typeof(string))
					{
						//check string
						var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
						if (marshal == null)
							throw new XunitException($"{method.Name}({paramType})");
						if (marshal.Value != UnmanagedType.LPStr)
							throw new XunitException($"{method.Name}({paramType})");
					}
					else if (paramType == typeof(string[]))
					{
						// check array of strings
						var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
						if (marshal == null)
							throw new XunitException($"{method.Name}({paramType})");
						if (marshal.Value != UnmanagedType.LPArray)
							throw new XunitException($"{method.Name}({paramType})");
						if (marshal.ArraySubType != UnmanagedType.LPStr)
							throw new XunitException($"{method.Name}({paramType})");
					}
					else
					{
						if (param.ParameterType.IsByRef || param.ParameterType.IsArray)
						{
							paramType = param.ParameterType.GetElementType();
						}

						var paramTypeInfo = paramType.GetTypeInfo();

						// the compiler will not alow invalid pointers, so we can skip those
						if (!paramTypeInfo.IsPointer)
						{
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
									throw new XunitException($"not blittable : {method.Name}({paramType})");
								}
							}
						}
					}
				}

				if (method.ReturnParameter.ParameterType == typeof(bool))
				{
					var marshal = method.ReturnParameter.GetCustomAttribute<MarshalAsAttribute>();
					if (marshal == null)
						throw new XunitException($"{method.Name}(return)");
					if (marshal.Value != UnmanagedType.I1)
						throw new XunitException($"{method.Name}(return)");
				}
			}
		}

		[SkippableFact]
		public void ExceptionsThrownInTheConstructorFailGracefully()
		{
			BrokenObject broken = null;
			try
			{
				broken = new BrokenObject();
			}
			catch (Exception)
			{
			}
			finally
			{
				broken?.Dispose();
				broken = null;
			}

			// try and trigger the finalizer
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private class BrokenObject : SKObject
		{
			public BrokenObject()
				: base(broken_native_method(), true)
			{
			}

			private static IntPtr broken_native_method()
			{
				throw new Exception("BREAK!");
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
			}
		}
	}
}
