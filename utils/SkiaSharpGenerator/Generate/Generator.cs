using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CppAst;

namespace SkiaSharpGenerator
{
	public class Generator : BaseTool
	{
		public Generator(string skiaRoot, string configFile, TextWriter outputWriter, DocumentationStore? docStore = null)
			: base(skiaRoot, configFile)
		{
			OutputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
			PreviousDocumentation = docStore;
		}

		public TextWriter OutputWriter { get; }

		public DocumentationStore? PreviousDocumentation { get; }

		public async Task GenerateAsync()
		{
			Log?.Log("Starting C# API generation...");

			config = await LoadConfigAsync(ConfigFile);

			PreviousDocumentation?.Load();

			LoadStandardMappings();

			ParseSkiaHeaders();

			UpdatingMappings();

			WriteApi(OutputWriter);

			Log?.Log("C# API generation complete.");
		}

		private void WriteApi(TextWriter writer)
		{
			Log?.LogVerbose("Writing C# API...");

			writer.WriteLine("using System;");
			writer.WriteLine("using System.Runtime.InteropServices;");
			writer.WriteLine("using System.Runtime.CompilerServices;");
			writer.WriteLine();
			WriteNamespaces(writer);
			writer.WriteLine();
			WriteClasses(writer);
			writer.WriteLine();
			writer.WriteLine($"#region Functions");
			writer.WriteLine();
			writer.WriteLine($"namespace {config.Namespace}");
			writer.WriteLine($"{{");
			writer.WriteLine($"\tinternal unsafe partial class {config.ClassName}");
			writer.WriteLine($"\t{{");
			WriteFunctions(writer);
			writer.WriteLine($"\t}}");
			writer.WriteLine($"}}");
			writer.WriteLine();
			writer.WriteLine($"#endregion Functions");
			writer.WriteLine();
			WriteDelegates(writer);
			writer.WriteLine();
			WriteStructs(writer);
			writer.WriteLine();
			WriteEnums(writer);
			writer.WriteLine();
			WriteDelegateProxies(writer);
		}

		private void WriteDelegates(TextWriter writer)
		{
			Log?.LogVerbose("  Writing delegates...");

			writer.WriteLine($"#region Delegates");
			writer.WriteLine($"#if !USE_LIBRARY_IMPORT");

			var delegates = compilation.Typedefs
				.Where(t => t.ElementType.TypeKind == CppTypeKind.Pointer)
				.Where(t => IncludeNamespace(t.GetDisplayName()))
				.OrderBy(t => t.GetDisplayName())
				.GroupBy(t => GetNamespace(t.GetDisplayName()));

			foreach (var group in delegates)
			{
				writer.WriteLine();
				writer.WriteLine($"namespace {group.Key} {{");

				foreach (var del in group)
				{
					WriteDelegate(writer, del);
				}

				writer.WriteLine($"}}");
			}

			writer.WriteLine();
			writer.WriteLine($"#endif // !USE_LIBRARY_IMPORT");
			writer.WriteLine($"#endregion");
		}

		private void WriteDelegate(TextWriter writer, CppTypedef del)
		{
			if (!(((CppPointerType)del.ElementType).ElementType is CppFunctionType function))
			{
				Log?.LogWarning($"Unknown delegate type {del}");

				writer.WriteLine($"// TODO: {del}");
				return;
			}

			var name = del.GetDisplayName();

			Log?.LogVerbose($"    {name}");

			functionMappings.TryGetValue(name, out var map);
			name = map?.CsType ?? CleanName(name);

			writer.WriteLine($"\t// {del}");
			writer.WriteLine($"\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");

			var (paramsList, returnType) = GetManagedFunctionArguments(function, map);
			if (returnType == "bool")
			{
				writer.WriteLine($"\t[return: MarshalAs (UnmanagedType.I1)]");
			}

			writer.WriteLine($"\tinternal unsafe delegate {returnType} {name}({string.Join(", ", paramsList)});");
			writer.WriteLine();
		}

		private void WriteStructs(TextWriter writer)
		{
			Log?.LogVerbose("  Writing structs...");

			writer.WriteLine($"#region Structs");

			var classes = compilation.Classes
				.Where(c => c.SizeOf != 0)
				.Where(c => IncludeNamespace(c.GetDisplayName()))
				.OrderBy(c => c.GetDisplayName())
				.GroupBy(c => GetNamespace(c.GetDisplayName()));

			foreach (var group in classes)
			{
				writer.WriteLine();
				writer.WriteLine($"namespace {group.Key} {{");

				foreach (var klass in group)
				{
					WriteStruct(writer, klass);
				}

				writer.WriteLine($"}}");
			}

			writer.WriteLine();
			writer.WriteLine($"#endregion");
		}

		private void WriteStruct(TextWriter writer, CppClass klass)
		{
			var cppClassName = klass.GetDisplayName();

			if (excludedTypes.Contains(cppClassName) == true)
			{
				Log?.LogVerbose($"    Skipping struct '{cppClassName}' because it was in the exclude list.");
				return;
			}

			Log?.LogVerbose($"    {cppClassName}");

			typeMappings.TryGetValue(cppClassName, out var map);
			var name = map?.CsType ?? CleanName(cppClassName);

			writer.WriteLine();
			writer.WriteLine($"\t// {cppClassName}");
			WriteDocIfAny(writer, DocumentationStore.Type(name), "\t");
			writer.WriteLine($"\t[StructLayout (LayoutKind.Sequential)]");
			var visibility = map?.IsInternal == true ? "internal" : "public";
			var isReadonly = map?.IsReadOnly == true ? " readonly" : "";
			var equatable = map?.GenerateEquality == true ? $" : IEquatable<{name}>" : "";
			writer.WriteLine($"\t{visibility}{isReadonly} unsafe partial struct {name}{equatable} {{");

			var allFields = new List<string>();
			foreach (var field in klass.Fields)
			{
				var type = GetType(field.Type);
				var funcPointerType = GetFunctionPointerType(field.Type);
				var cppT = GetCppType(field.Type);

				writer.WriteLine($"\t\t// {field}");

				var fieldName = field.Name;
				var isPrivate = fieldName.StartsWith("_private_", StringComparison.OrdinalIgnoreCase);
				if (isPrivate)
					fieldName = fieldName[9..];
				isPrivate |= fieldName.StartsWith("reserved", StringComparison.OrdinalIgnoreCase);

				allFields.Add(fieldName);

				WriteDocIfAny(writer, DocumentationStore.Field(name, fieldName), "\t\t");

				var vis = map?.IsInternal == true ? "public" : "private";
				var ro = map?.IsReadOnly == true ? " readonly" : "";
				if (funcPointerType is null)
				{
					writer.WriteLine($"\t\t{vis}{ro} {type} {fieldName};");
				}
				else
				{
					writer.WriteLine($"#if USE_LIBRARY_IMPORT");
					writer.WriteLine($"\t\t{vis}{ro} {funcPointerType} {fieldName};");
					writer.WriteLine($"#else");
					writer.WriteLine($"\t\t{vis}{ro} {type} {fieldName};");
					writer.WriteLine($"#endif");
				}

				if (!isPrivate && (map == null || (map.GenerateProperties && !map.IsInternal)))
				{
					var propertyName = fieldName;
					if (map != null && map.Members.TryGetValue(propertyName, out var fieldMap))
					{
						if (string.IsNullOrEmpty(fieldMap))
							isPrivate = true;
						propertyName = fieldMap;
					}
					else
					{
						propertyName = CleanName(propertyName);
					}

					if (!isPrivate)
					{
						WriteDocIfAny(writer, DocumentationStore.Property(name, propertyName), "\t\t");

						if (fieldName == "value")
							fieldName = "this." + fieldName;

						if (cppT == "bool")
						{
							if (map?.IsReadOnly == true)
							{
								writer.WriteLine($"\t\tpublic readonly bool {propertyName} => {fieldName} > 0;");
							}
							else
							{
								writer.WriteLine($"\t\tpublic bool {propertyName} {{");
								writer.WriteLine($"\t\t\treadonly get => {fieldName} > 0;");
								writer.WriteLine($"\t\t\tset => {fieldName} = value ? (byte)1 : (byte)0;");
								writer.WriteLine($"\t\t}}");
							}
						}
						else
						{
							if (map?.IsReadOnly == true)
							{
								if (funcPointerType is null)
								{
									writer.WriteLine($"\t\tpublic readonly {type} {propertyName} => {fieldName};");
								}
								else
								{
									writer.WriteLine($"#if USE_LIBRARY_IMPORT");
									writer.WriteLine($"\t\tpublic readonly {funcPointerType} {propertyName} => {fieldName};");
									writer.WriteLine($"#else");
									writer.WriteLine($"\t\tpublic readonly {type} {propertyName} => {fieldName};");
									writer.WriteLine($"#endif");
								}
							}
							else
							{
								writer.WriteLine($"\t\tpublic {type} {propertyName} {{");
								writer.WriteLine($"\t\t\treadonly get => {fieldName};");
								writer.WriteLine($"\t\t\tset => {fieldName} = value;");
								writer.WriteLine($"\t\t}}");
							}
						}
					}
				}

				writer.WriteLine();
			}

			if (map?.GenerateEquality == true)
			{
				// IEquatable
				var equalityFields = new List<string>();
				foreach (var f in allFields)
				{
					equalityFields.Add($"{f} == obj.{f}");
				}
				WriteDocIfAny(writer, DocumentationStore.Method(name, "Equals", [name]), "\t\t");
				writer.WriteLine($"\t\tpublic readonly bool Equals ({name} obj) =>");
				writer.WriteLine($"#pragma warning disable CS8909");
				writer.WriteLine($"\t\t\t{string.Join(" && ", equalityFields)};");
				writer.WriteLine($"#pragma warning restore CS8909");
				writer.WriteLine();

				// Equals
				WriteDocIfAny(writer, DocumentationStore.Method(name, "Equals", ["object"]), "\t\t");
				writer.WriteLine($"\t\tpublic readonly override bool Equals (object obj) =>");
				writer.WriteLine($"\t\t\tobj is {name} f && Equals (f);");
				writer.WriteLine();

				// equality operators
				WriteDocIfAny(writer, DocumentationStore.Method(name, "op_Equality", [name, name]), "\t\t");
				writer.WriteLine($"\t\tpublic static bool operator == ({name} left, {name} right) =>");
				writer.WriteLine($"\t\t\tleft.Equals (right);");
				writer.WriteLine();
				WriteDocIfAny(writer, DocumentationStore.Method(name, "op_Inequality", [name, name]), "\t\t");
				writer.WriteLine($"\t\tpublic static bool operator != ({name} left, {name} right) =>");
				writer.WriteLine($"\t\t\t!left.Equals (right);");
				writer.WriteLine();

				// GetHashCode
				WriteDocIfAny(writer, DocumentationStore.Method(name, "GetHashCode", Array.Empty<string>()), "\t\t");
				writer.WriteLine($"\t\tpublic readonly override int GetHashCode ()");
				writer.WriteLine($"\t\t{{");
				writer.WriteLine($"\t\t\tvar hash = new HashCode ();");
				foreach (var f in allFields)
				{
					writer.WriteLine($"\t\t\thash.Add ({f});");
				}
				writer.WriteLine($"\t\t\treturn hash.ToHashCode ();");
				writer.WriteLine($"\t\t}}");
				writer.WriteLine();
			}

			writer.WriteLine($"\t}}");
		}

		private void WriteEnums(TextWriter writer)
		{
			Log?.LogVerbose("  Writing enums...");

			writer.WriteLine($"#region Enums");

			var enums = compilation.Enums
				.Where(e => IncludeNamespace(e.GetDisplayName()))
				.OrderBy(e => e.GetDisplayName())
				.GroupBy(e => GetNamespace(e.GetDisplayName()));

			foreach (var group in enums)
			{
				writer.WriteLine();
				writer.WriteLine($"namespace {group.Key} {{");

				foreach (var enm in group)
				{
					WriteEnum(writer, enm);
				}

				writer.WriteLine($"}}");
			}

			writer.WriteLine();
			writer.WriteLine($"#endregion");
		}

		private void WriteEnum(TextWriter writer, CppEnum enm)
		{
			var cppEnumName = enm.GetDisplayName();

			if (string.IsNullOrEmpty(cppEnumName))
			{
				Log?.LogWarning($"Unknown enum type {enm}");
				return;
			}

			typeMappings.TryGetValue(cppEnumName, out var map);
			if (map?.Generate == false)
				return;

			Log?.LogVerbose($"    {cppEnumName}");

			var name = map?.CsType ?? CleanName(cppEnumName);

			var visibility = "public";
			if (map?.IsInternal == true)
				visibility = "internal";

			writer.WriteLine();
			writer.WriteLine($"\t// {cppEnumName}");
			WriteDocIfAny(writer, DocumentationStore.Type(name), "\t");
			if (map?.IsObsolete == true)
				writer.WriteLine($"\t[Obsolete]");
			if (map?.IsFlags == true)
				writer.WriteLine($"\t[Flags]");
			writer.WriteLine($"\t{visibility} enum {name} {{");
			foreach (var field in enm.Items)
			{
				var fieldName = field.Name;
				if (map != null && map.Members.TryGetValue(fieldName, out var fieldMap))
					fieldName = fieldMap;
				else
					fieldName = CleanEnumFieldName(fieldName, cppEnumName);

				if (string.IsNullOrEmpty(fieldName))
					continue;

				var commentVal = field.ValueExpression?.ToString();
				if (string.IsNullOrEmpty(commentVal))
					commentVal = field.Value.ToString();

				writer.WriteLine($"\t\t// {field.Name} = {commentVal}");
				WriteDocIfAny(writer, DocumentationStore.Field(name, fieldName), "\t\t");
				writer.WriteLine($"\t\t{fieldName} = {field.Value},");
			}
			writer.WriteLine($"\t}}");
		}

		private void WriteNamespaces(TextWriter writer)
		{
			Log?.LogVerbose("  Writing namespaces...");

			writer.WriteLine($"#region Namespaces");
			writer.WriteLine();

			var namspaces = config.Namespaces.Values;
			foreach (var ns in namspaces)
			{
				if (string.IsNullOrEmpty(ns.CsName))
					continue;

				var full = $"{config.Namespace}.{ns.CsName}";

				Log?.LogVerbose($"    {full}");

				writer.WriteLine($"using {full};");
			}

			writer.WriteLine();
			writer.WriteLine($"#endregion");
		}

		private void WriteClasses(TextWriter writer)
		{
			Log?.LogVerbose("  Writing usings...");

			writer.WriteLine($"#region Class declarations");
			writer.WriteLine();

			var classes = compilation.Classes
				.OrderBy(c => c.GetDisplayName())
				.ToList();
			foreach (var klass in classes)
			{
				var type = klass.GetDisplayName();
				skiaTypes.Add(type, klass.SizeOf != 0);

				Log?.LogVerbose($"    {klass.GetDisplayName()}");

				if (klass.SizeOf == 0)
					writer.WriteLine($"using {klass.GetDisplayName()} = System.IntPtr;");
			}

			writer.WriteLine();
			writer.WriteLine($"#endregion");
		}

		private void WriteFunctions(TextWriter writer)
		{
			Log?.LogVerbose("  Writing p/invokes...");

			var functionGroups = compilation.Functions
				.Where(f => IncludeNamespace(f.Name))
				.OrderBy(f => f.Name)
				.GroupBy(f => f.Span.Start.File.ToLower().Replace("\\", "/"))
				.OrderBy(g => Path.GetDirectoryName(g.Key) + "/" + Path.GetFileName(g.Key));

			foreach (var group in functionGroups)
			{
				var fullPath = Path.GetFullPath(group.Key).ToLower();
				if (excludedFiles.Any(e => Path.GetFullPath(e).ToLower() == fullPath))
				{
					Log?.LogVerbose($"    Skipping file '{group.Key}' because it was in the exclude list.");
					continue;
				}

				writer.WriteLine($"\t\t#region {Path.GetFileName(group.Key)}");
				foreach (var function in group)
				{
					Log?.LogVerbose($"    {function.Name}");

					var name = function.Name;
					functionMappings.TryGetValue(name, out var funcMap);
					var skipFunction = false;

					var paramsList = new List<string>();
					var paramsListWithFuncPointers = new List<string>();
					var paramNamesList = new List<string>();
					for (var i = 0; i < function.Parameters.Count; i++)
					{
						var p = function.Parameters[i];
						var n = string.IsNullOrEmpty(p.Name) ? $"param{i}" : p.Name;
						n = SafeName(n);
						var t1 = GetType(p.Type);
						var t2 = GetFunctionPointerType(p.Type);

						// Mono WASM didn't support function pointers in DllImport definitions until .NET 8.
						// While it should, it still didn't work for me even on .NET 9 previews, so keeping `void*` instead of function pointers.
						// It makes higher chance of accident mistakes, but old managed delegates build should catch the errors compile time too.
						if (t2 is not null) t2 = "void*";
						t2 ??= t1;

						var cppT = GetCppType(p.Type);
						if (excludedTypes.Contains(cppT) == true)
						{
							Log?.LogVerbose($"    Skipping function '{function.Name}' because parameter '{cppT}' was in the exclude list.");
							skipFunction = true;
							break;
						}
						if (t1 == "Boolean" || cppT == "bool")
							t1 = t2 = "[MarshalAs (UnmanagedType.I1)] bool";
						if (funcMap != null && funcMap.Parameters.TryGetValue(i.ToString(), out var newT))
							t1 = t2 = newT;
						paramsList.Add($"{t1} {n}");
						paramsListWithFuncPointers.Add($"{t2} {n}");
						paramNamesList.Add(n);
					}

					if (skipFunction)
						continue;

					var returnType = GetType(function.ReturnType);
					var retAttr = "";
					if (funcMap != null && funcMap.Parameters.TryGetValue("-1", out var newR))
					{
						returnType = newR;
					}
					else if (returnType == "Boolean" || GetCppType(function.ReturnType) == "bool")
					{
						returnType = "bool";
						retAttr = $"[return: MarshalAs (UnmanagedType.I1)]";
					}

					writer.WriteLine();
					writer.WriteLine($"\t\t// {function}");
					writer.WriteLine($"\t\t#if !USE_DELEGATES");
					writer.WriteLine($"\t\t#if USE_LIBRARY_IMPORT");
					writer.WriteLine($"\t\t[LibraryImport ({config.DllName})]");
					if (!string.IsNullOrEmpty(retAttr))
						writer.WriteLine($"\t\t{retAttr}");
					writer.WriteLine($"\t\tinternal static partial {returnType} {name} ({string.Join(", ", paramsListWithFuncPointers)});");

					writer.WriteLine($"\t\t#else // !USE_LIBRARY_IMPORT");
					writer.WriteLine($"\t\t[DllImport ({config.DllName}, CallingConvention = CallingConvention.Cdecl)]");
					if (!string.IsNullOrEmpty(retAttr))
						writer.WriteLine($"\t\t{retAttr}");
					writer.WriteLine($"\t\tinternal static extern {returnType} {name} ({string.Join(", ", paramsList)});");
					writer.WriteLine($"\t\t#endif");

					writer.WriteLine($"\t\t#else");
					writer.WriteLine($"\t\tprivate partial class Delegates {{");
					writer.WriteLine($"\t\t\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");
					if (!string.IsNullOrEmpty(retAttr))
						writer.WriteLine($"\t\t\t{retAttr}");
					writer.WriteLine($"\t\t\tinternal delegate {returnType} {name} ({string.Join(", ", paramsList)});");
					writer.WriteLine($"\t\t}}");
					writer.WriteLine($"\t\tprivate static Delegates.{name} {name}_delegate;");
					writer.WriteLine($"\t\tinternal static {returnType} {name} ({string.Join(", ", paramsList)}) =>");
					writer.WriteLine($"\t\t\t({name}_delegate ??= GetSymbol<Delegates.{name}> (\"{name}\")).Invoke ({string.Join(", ", paramNamesList)});");
					writer.WriteLine($"\t\t#endif");
				}
				writer.WriteLine();
				writer.WriteLine($"\t\t#endregion");
				writer.WriteLine();
			}
		}

		public void WriteDelegateProxies(TextWriter writer)
		{
			Log?.LogVerbose("  Writing delegate proxies...");

			writer.WriteLine($"#region DelegateProxies");

			var delegates = compilation.Typedefs
				.Where(t => t.ElementType.TypeKind == CppTypeKind.Pointer)
				.Where(t => IncludeNamespace(t.GetDisplayName()))
				.OrderBy(t => t.GetDisplayName())
				.GroupBy(t => GetNamespace(t.GetDisplayName()));

			foreach (var group in delegates)
			{
				writer.WriteLine();
				writer.WriteLine($"namespace {group.Key} {{");
				writer.WriteLine($"internal static unsafe partial class DelegateProxies {{ ");

				foreach (var del in group)
				{
					WriteDelegateProxy(writer, del);
				}

				writer.WriteLine($"}}");
				writer.WriteLine($"}}");
			}

			writer.WriteLine();
			writer.WriteLine($"#endregion");
		}

		private void WriteDelegateProxy(TextWriter writer, CppTypedef del)
		{
			if (!(((CppPointerType)del.ElementType).ElementType is CppFunctionType function))
			{
				Log?.LogWarning($"Unknown delegate type {del}");

				writer.WriteLine($"// TODO: {del}");
				return;
			}

			var nativeName = del.GetDisplayName();

			Log?.LogVerbose($"    {nativeName}");

			functionMappings.TryGetValue(nativeName, out var map);
			var name = map?.CsType ?? CleanName(nativeName);

			if (map?.GenerateProxy == false)
			{
				return;
			}

			var functionPointerType = GetFunctionPointerType(del, map);

			var (paramsList, returnType) = GetManagedFunctionArguments(function, map);

			var proxies = map?.ProxySuffixes ?? new List<string> { "" };

			foreach (var proxyPrefix in proxies)
			{
				var proxyName = name.EndsWith("ProxyDelegate") ? name.Replace("ProxyDelegate", "Proxy") : name;
				var implName = name.EndsWith("ProxyDelegate") ? name.Replace("ProxyDelegate", "ProxyImplementation") : name + "Implementation";

				proxyName += proxyPrefix;
				implName += proxyPrefix;

				writer.WriteLine($"\t/// Proxy for {nativeName} native function.");
				writer.WriteLine($"#if USE_LIBRARY_IMPORT");
				writer.WriteLine($"\tpublic static readonly {functionPointerType} {proxyName} = &{implName};");
				writer.WriteLine($"\t[UnmanagedCallersOnly(CallConvs = new [] {{typeof(CallConvCdecl)}})]");
				writer.WriteLine($"#else");
				writer.WriteLine($"\tpublic static readonly {name} {proxyName} = {implName};");
				writer.WriteLine($"\t[MonoPInvokeCallback (typeof ({name}))]");
				writer.WriteLine($"#endif");
				if (returnType == "bool") writer.WriteLine($"\t[return: MarshalAs (UnmanagedType.I1)]");
				writer.WriteLine($"\tprivate static partial {returnType} {implName}({string.Join(",", paramsList)});");
				writer.WriteLine();
			}
		}

		private void WriteDocIfAny(TextWriter writer, string key, string indent)
		{
			if (PreviousDocumentation?.TryGet(key, out var xml) != true)
				return;

			foreach (var line in xml.Split('\n'))
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				writer.Write(indent);
				writer.WriteLine(line.TrimEnd());
			}
		}
	}
}
