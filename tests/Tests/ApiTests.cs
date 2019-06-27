﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace SkiaSharp.Tests
{
	public class ApiTest : BaseTest
	{
		private static IEnumerable<Type> InteropApiTypes => new[]
			{
				typeof(SkiaSharp.SKNativeObject).Assembly.GetType("SkiaSharp.SkiaApi"),
				typeof(HarfBuzzSharp.NativeObject).Assembly.GetType("HarfBuzzSharp.HarfBuzzApi")
			};

		private static IEnumerable<MethodInfo> InteropMembers =>
			InteropApiTypes
			.SelectMany(t => t.GetMethods())
			.Where(a => a.GetCustomAttribute<DllImportAttribute>() != null)
			.Distinct();

		private static IEnumerable<Type> InteropDelegates =>
			InteropMembers.SelectMany(m =>
				m.GetParameters()
				.Select(p => p.ParameterType)
				.Where(t => typeof(Delegate).IsAssignableFrom(t)))
			.Distinct();

		public static IEnumerable<object[]> InteropMembersData =>
			Enumerable.Union(
				InteropMembers.Select(m => new object[] { m, null }),
				InteropDelegates.Select(t => new object[] { t.GetMethod("Invoke"), t.Name }));

		public static IEnumerable<object[]> InteropDelegatesData =>
			InteropDelegates.Select(m => new object[] { m });

		[SkippableTheory]
		[MemberData(nameof(InteropDelegatesData))]
		public void DelegateTypesHaveAttributes(Type delegateType)
		{
			Assert.NotNull(delegateType.GetCustomAttribute<UnmanagedFunctionPointerAttribute>());
		}

		[SkippableTheory]
		[MemberData(nameof(InteropMembersData))]
		public void ApiTypesAreNotInvalid(MethodInfo method, string delegateName)
		{
			var del = string.IsNullOrEmpty(delegateName) ? "" : $" for delegate '{delegateName}'";

			var ass = method.DeclaringType.Assembly;

			foreach (var param in method.GetParameters())
			{
				// get the real parameter type
				var paramType = param.ParameterType;
				if (param.ParameterType.IsByRef || param.ParameterType.IsArray)
					paramType = param.ParameterType.GetElementType();

				// check to make sure that the "internal" versions are being used
				var internalType = ass.GetType(paramType.FullName + "Internal");
				var nativeType = ass.GetType(paramType.FullName + "Native");
				if (internalType != null || nativeType != null)
					throw new XunitException($"Using type {paramType.FullName}{del}, but type {(internalType ?? nativeType).FullName} exists.");
			}
		}

		[SkippableTheory]
		[MemberData(nameof(InteropMembersData))]
		public void ApiReturnTypesArePrimitives(MethodInfo method, string delegateName)
		{
			var del = string.IsNullOrEmpty(delegateName) ? "" : $" for delegate '{delegateName}'";

			var returnType = method.ReturnType;

			if (returnType == typeof(bool))
			{
				var marshal = method.ReturnParameter.GetCustomAttribute<MarshalAsAttribute>();
				if (marshal?.Value != UnmanagedType.I1)
					throw new XunitException($"Boolean return{del} does not have [MarshalAs(I1)] attribute.");
			}
			else
			{
				var prim = returnType.IsPrimitive;
				var enm = returnType.IsEnum;
				var ptr = returnType.IsPointer;
				var voidType = returnType == typeof(void);

				if (!prim && !enm && !ptr && !voidType)
					throw new XunitException($"Method return{del} is neither primitive, an enum, a pointer nor void.");
			}
		}

		[SkippableTheory]
		[MemberData(nameof(InteropMembersData))]
		public void ApiTypesAreMarshalledCorrectly(MethodInfo method, string delegateName)
		{
			var del = string.IsNullOrEmpty(delegateName) ? "" : $" for delegate '{delegateName}'";

			foreach (var param in method.GetParameters())
			{
				var paramType = param.ParameterType;
				var isLocalType = paramType.Namespace == "SkiaSharp" || paramType.Namespace == "HarfBuzzSharp";

				if (paramType == typeof(bool))
				{
					//check bool
					var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
					if (marshal?.Value != UnmanagedType.I1)
						throw new XunitException($"Boolean parameter '{param.Name}'{del} does not have [MarshalAs(I1)] attribute.");
				}
				if (paramType == typeof(string))
				{
					//check string
					var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
					if (marshal?.Value != UnmanagedType.LPStr)
						throw new XunitException($"String parameter '{param.Name}'{del} does not have [MarshalAs(LPStr)] attribute.");
				}
				else if (paramType == typeof(string[]))
				{
					// check array of strings
					var marshal = param.GetCustomAttribute<MarshalAsAttribute>();
					if (marshal?.Value != UnmanagedType.LPArray || marshal?.ArraySubType != UnmanagedType.LPStr)
						throw new XunitException($"String array parameter '{param.Name}'{del} does not have [MarshalAs(LPArray, ArraySubType = LPStr)] attribute.");
				}
				else
				{
					if (param.ParameterType.IsByRef || param.ParameterType.IsArray)
						paramType = param.ParameterType.GetElementType();

					// the compiler will not alow invalid pointers, so we can skip those
					if (!paramType.IsPointer && !typeof(Delegate).IsAssignableFrom(paramType))
					{
						// make sure only structs or special types
						var isClass =
							paramType.GetTypeInfo().IsClass &&
							paramType.FullName != typeof(StringBuilder).FullName;
						Assert.False(isClass, $"Parameter type '{paramType.FullName}'{del} is not a struct.");

						// make sure our structs have a layout type
						if (!paramType.GetTypeInfo().IsEnum && isLocalType)
						{
							// check blittable
							try
							{
								GCHandle.Alloc(Activator.CreateInstance(paramType), GCHandleType.Pinned).Free();
							}
							catch
							{
								throw new XunitException($"Parameter type '{paramType.FullName}'{del} is not a blittable type.");
							}
						}
					}
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
