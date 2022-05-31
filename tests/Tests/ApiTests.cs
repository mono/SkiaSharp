using System;
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
			typeof(SkiaSharp.SkiaApi),
			typeof(HarfBuzzSharp.HarfBuzzApi),
			typeof(SkiaSharp.SceneGraphApi),
			typeof(SkiaSharp.SkottieApi),
		};

		private static IEnumerable<Type> InteropApiDelegatesTypes => new[]
		{
			typeof(SkiaSharp.SkiaApi).Assembly.GetType("SkiaSharp.SkiaApi+Delegates"),
			typeof(HarfBuzzSharp.HarfBuzzApi).Assembly.GetType("HarfBuzzSharp.HarfBuzzApi+Delegates"),
			typeof(SkiaSharp.SceneGraphApi).Assembly.GetType("SkiaSharp.SceneGraphApi+Delegates"),
			typeof(SkiaSharp.SkottieApi).Assembly.GetType("SkiaSharp.SkottieApi+Delegates"),
		};

		private static IEnumerable<MethodInfo> InteropMembers =>
			InteropApiTypes
			.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
			.Where(a => a.GetCustomAttribute<DllImportAttribute>() != null)
			.Distinct();

		private static IEnumerable<Type> InteropNestedDelegates =>
			InteropApiDelegatesTypes
			.Where(t => t != null) // may not be found in platforms other than net4x
			.SelectMany(t => t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
			.Where(t => typeof(Delegate).IsAssignableFrom(t))
			.Distinct();

		private static IEnumerable<Type> InteropDelegates =>
			InteropMembers.SelectMany(m =>
				m.GetParameters()
				.Select(p => p.ParameterType)
				.Where(t => typeof(Delegate).IsAssignableFrom(t)))
			.Union(InteropNestedDelegates)
			.Distinct();

		public static IEnumerable<object[]> InteropMembersData =>
			Enumerable.Union(
				InteropMembers.Select(m => new object[] { m, null }),
				InteropDelegates.Select(t => new object[] { t.GetMethod("Invoke"), t.Name }));

		public static IEnumerable<object[]> InteropDelegatesData =>
			InteropDelegates.Select(m => new object[] { m });

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void DelegateTypesAreValid()
		{
			var del = InteropDelegatesData;
			Assert.NotEmpty(del);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableTheory]
		[MemberData(nameof(InteropDelegatesData))]
		public void DelegateTypesHaveAttributes(Type delegateType)
		{
			Assert.NotNull(delegateType.GetCustomAttribute<UnmanagedFunctionPointerAttribute>());
		}

		[Trait(CategoryKey, ApiCategory)]
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

		[Trait(CategoryKey, ApiCategory)]
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

		[Trait(CategoryKey, ApiCategory)]
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

						// special cases where we know the type is not blittable, but can be passed to native code
						var isSkippedType =
							paramType.FullName != typeof(SKManagedStreamDelegates).FullName &&
							paramType.FullName != typeof(SKManagedWStreamDelegates).FullName &&
							paramType.FullName != typeof(SKManagedDrawableDelegates).FullName &&
							paramType.FullName != typeof(SKManagedTraceMemoryDumpDelegates).FullName &&
							paramType.FullName != typeof(GRVkBackendContextNative).FullName; // TODO: this type probably needs better checks as it is not 100% delegates

						// make sure our structs have a layout type
						if (!paramType.GetTypeInfo().IsEnum && isLocalType && isSkippedType)
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

		[Trait(CategoryKey, ApiCategory)]
		[SkippableTheory]
		// too old
		[InlineData("80.0", "0.0", "[80.0, 81.0)")]
		// same version
		[InlineData("68.0", "68.0")]
		// older C API
		[InlineData("68.3", "68.0", "[68.3, 69.0)")]
		[InlineData("68.3", "68.2", "[68.3, 69.0)")]
		[InlineData("80.2", "80.0", "[80.2, 81.0)")]
		// older skia milestone
		[InlineData("68.0", "60.0", "[68.0, 69.0)")]
		[InlineData("68.3", "60.0", "[68.3, 69.0)")]
		[InlineData("68.3", "60.3", "[68.3, 69.0)")]
		[InlineData("68.3", "60.9", "[68.3, 69.0)")]
		// newer skia milestone
		[InlineData("68.0", "80.0", "[68.0, 69.0)")]
		[InlineData("68.3", "80.0", "[68.3, 69.0)")]
		[InlineData("68.0", "80.3", "[68.0, 69.0)")]
		[InlineData("68.3", "80.3", "[68.3, 69.0)")]
		// newer C API
		[InlineData("80.0", "80.0")]
		[InlineData("80.0", "80.1")]
		[InlineData("80.0", "80.2")]
		[InlineData("80.0", "80.4")]
		[InlineData("80.0", "80.9")]
		// possibly bad compliations
		[InlineData("0.0", "80.0")]
		[InlineData("0.0", "0.0")]
		public void CheckNativeLibraryCompatible(string minimum, string current, string exception = null)
		{
			var m = new Version(minimum);
			var c = new Version(current);

			var compatible = exception == null;

			Assert.Equal(compatible, SkiaSharpVersion.CheckNativeLibraryCompatible(m, c));
			if (!compatible)
			{
				var ex = Assert.Throws<InvalidOperationException>(() => SkiaSharpVersion.CheckNativeLibraryCompatible(m, c, true));
				Assert.Contains(exception, ex.Message);
			}
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void TestLibraryVersions()
		{
			Assert.True(SkiaSharpVersion.CheckNativeLibraryCompatible());
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void TestLibraryVersionsDoesNotThrow()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible(true);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void TestVersionsString()
		{
			Assert.Equal(SkiaSharpVersion.Native.ToString(2), SkiaSharpVersion.NativeString);
		}

		[Trait(CategoryKey, ApiCategory)]
		[SkippableFact]
		public void PlatformConfigurationIsMuslOverrideCanBeFoundViaReflection()
		{
			var assembly = typeof(SkiaSharpVersion).Assembly;
			var config = assembly.DefinedTypes.FirstOrDefault(t => t.Name == "PlatformConfiguration");
			var overrideProp = config.GetProperty("LinuxFlavor");

			Assert.Equal(typeof(string), overrideProp.PropertyType);
			Assert.True(overrideProp.CanRead);
			Assert.True(overrideProp.CanWrite);
		}
	}
}
