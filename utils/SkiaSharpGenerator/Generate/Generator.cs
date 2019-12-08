using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CppAst;

namespace SkiaSharpGenerator
{
	public class Generator
	{
		private readonly Dictionary<string, TypeMapping> typeMappings = new Dictionary<string, TypeMapping>();
		private readonly Dictionary<string, FunctionMapping> functionMappings = new Dictionary<string, FunctionMapping>();
		private readonly Dictionary<string, bool> skiaTypes = new Dictionary<string, bool>();

		private CppCompilation compilation = new CppCompilation();
		private Config config = new Config();

		public Generator(string skiaRoot, string configFile, TextWriter outputWriter)
		{
			SkiaRoot = skiaRoot ?? throw new ArgumentNullException(nameof(skiaRoot));
			ConfigFile = configFile ?? throw new ArgumentNullException(nameof(configFile));
			OutputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
		}

		public ILogger? Log { get; set; }

		public string SkiaRoot { get; }

		public string ConfigFile { get; }

		public TextWriter OutputWriter { get; }

		public bool HasErrors =>
			compilation?.Diagnostics?.HasErrors ?? false;

		public IEnumerable<CppDiagnosticMessage> Messages =>
			compilation?.Diagnostics?.Messages ?? Array.Empty<CppDiagnosticMessage>();

		public async Task GenerateAsync()
		{
			Log?.Log("Starting C# API generation...");

			config = await LoadConfigAsync(ConfigFile);

			LoadStandardMappings();

			ParseSkiaHeaders();

			UpdatingMappings();

			WriteApi(OutputWriter);

			Log?.Log("C# API generation complete.");
		}

		private void ParseSkiaHeaders()
		{
			Log?.LogVerbose("Parsing skia headers...");

			var options = new CppParserOptions();

			foreach (var header in config.IncludeDirs)
			{
				var path = Path.Combine(SkiaRoot, header);
				options.IncludeFolders.Add(path);
			}

			var headers = new List<string>();
			foreach (var header in config.Headers)
			{
				var path = Path.Combine(SkiaRoot, header.Key);
				options.IncludeFolders.Add(path);
				foreach (var filter in header.Value)
				{
					headers.AddRange(Directory.EnumerateFiles(path, filter));
				}
			}

			compilation = CppParser.ParseFiles(headers, options);

			if (compilation == null || compilation.HasErrors)
			{
				Log?.LogError("Parsing headers failed.");
				throw new Exception("Parsing headers failed.");
			}
		}

		private void LoadStandardMappings()
		{
			Log?.LogVerbose("Loading standard types...");

			var standardMappings = new Dictionary<string, string>
			{
				// stdint.h types:
				{ "uint8_t",              nameof(Byte) },
				{ "uint16_t",             nameof(UInt16) },
				{ "uint32_t",             nameof(UInt32) },
				{ "uint64_t",             nameof(UInt64) },
				{ "usize_t" ,             "/* usize_t */ " + nameof(UIntPtr) },
				{ "uintptr_t" ,           nameof(UIntPtr) },
				{ "int8_t",               nameof(SByte) },
				{ "int16_t",              nameof(Int16) },
				{ "int32_t",              nameof(Int32) },
				{ "int64_t",              nameof(Int64) },
				{ "size_t" ,              "/* size_t */ " + nameof(IntPtr) },
				{ "intptr_t" ,            nameof(IntPtr) },

				// standard types:
				{ "bool",                 nameof(Byte) },
				{ "char",                 "/* char */ void" },
				{ "unsigned char",        "/* unsigned char */ void" },
				{ "signed char",          "/* signed char */ void" },
				{ "short",                nameof(Int16) },
				{ "short int",            nameof(Int16) },
				{ "signed short",         nameof(Int16) },
				{ "signed short int",     nameof(Int16) },
				{ "unsigned short",       nameof(UInt16) },
				{ "unsigned short int",   nameof(UInt16) },
				{ "int",                  nameof(Int32) },
				{ "signed",               nameof(Int32) },
				{ "signed int",           nameof(Int32) },
				{ "unsigned",             nameof(UInt32) },
				{ "unsigned int",         nameof(UInt32) },
				{ "long",                 nameof(Int64) },
				{ "long int",             nameof(Int64) },
				{ "signed long",          nameof(Int64) },
				{ "signed long int",      nameof(Int64) },
				{ "unsigned long",        nameof(UInt64) },
				{ "unsigned long int",    nameof(UInt64) },
				{ "float",                nameof(Single) },
				{ "double",               nameof(Double) },
				// TODO: long double, wchar_t ?

				{ "void",                 "void" },
			};

			foreach (var mapping in standardMappings)
			{
				var map = new TypeMapping { CsType = mapping.Value };
				typeMappings[mapping.Key] = map;
			}
		}

		private void UpdatingMappings()
		{
			// load all the classes/structs
			var typedefs = compilation.Classes;
			foreach (var klass in typedefs)
			{
				typeMappings[klass.GetDisplayName()] = new TypeMapping();
			}

			// load all the enums
			var enums = compilation.Enums;
			foreach (var enm in enums)
			{
				typeMappings[enm.GetDisplayName()] = new TypeMapping();
			}

			// load the mapping file
			foreach (var mapping in config.Mappings.Types)
			{
				typeMappings[mapping.Key] = mapping.Value;
			}
			foreach (var mapping in config.Mappings.Functions)
			{
				functionMappings[mapping.Key] = mapping.Value;
			}
		}

		private async Task<Config> LoadConfigAsync(string configPath)
		{
			Log?.LogVerbose("Loading configuration...");

			using var configJson = File.OpenRead(configPath);

			return await JsonSerializer.DeserializeAsync<Config>(configJson, new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
			});
		}

		private void WriteApi(TextWriter writer)
		{
			Log?.LogVerbose("Writing C# API...");

			writer.WriteLine("using System;");
			writer.WriteLine("using System.Runtime.InteropServices;");
			writer.WriteLine();
			writer.WriteLine($"namespace {config.Namespace}");
			writer.WriteLine($"{{");
			WriteClasses(writer);
			writer.WriteLine();
			writer.WriteLine($"\tinternal unsafe partial class {config.ClassName}");
			writer.WriteLine($"\t{{");
			WriteFunctions(writer);
			writer.WriteLine($"\t}}");
			writer.WriteLine();
			WriteDelegates(writer);
			writer.WriteLine();
			WriteStructs(writer);
			writer.WriteLine();
			WriteEnums(writer);
			writer.WriteLine($"}}");
		}

		private void WriteDelegates(TextWriter writer)
		{
			Log?.LogVerbose("  Writing delegates...");

			writer.WriteLine($"\t#region Delegates");

			var delegates = compilation.Typedefs
				.Where(t => t.ElementType.TypeKind == CppTypeKind.Pointer)
				.OrderBy(t => t.GetDisplayName());
			foreach (var del in delegates)
			{
				if (!(((CppPointerType)del.ElementType).ElementType is CppFunctionType function))
				{
					Log?.LogWarning($"Unknown delegate type {del}");

					writer.WriteLine($"// TODO: {del}");
					continue;
				}

				Log?.LogVerbose($"    {del.GetDisplayName()}");

				var name = del.GetDisplayName();
				functionMappings.TryGetValue(name, out var map);
				name = map?.CsType ?? Utils.CleanName(name);

				writer.WriteLine();
				writer.WriteLine($"\t// {del}");
				writer.WriteLine($"\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");

				var paramsList = new List<string>();
				for (var i = 0; i < function.Parameters.Count; i++)
				{
					var p = function.Parameters[i];
					var n = string.IsNullOrEmpty(p.Name) ? $"param{i}" : p.Name;
					var t = GetType(p.Type);
					var cppT = GetCppType(p.Type);
					if (cppT == "bool")
						t = $"[MarshalAs (UnmanagedType.I1)] bool";
					if (map != null && map.Parameters.TryGetValue(i.ToString(), out var newT))
						t = newT;
					paramsList.Add($"{t} {n}");
				}

				var returnType = GetType(function.ReturnType);
				if (map != null && map.Parameters.TryGetValue("-1", out var newR))
				{
					returnType = newR;
				}
				else if (GetCppType(function.ReturnType) == "bool")
				{
					returnType = "bool";
					writer.WriteLine($"\t[return: MarshalAs (UnmanagedType.I1)]");
				}

				writer.WriteLine($"\tinternal unsafe delegate {returnType} {name}({string.Join(", ", paramsList)});");
			}

			writer.WriteLine();
			writer.WriteLine($"\t#endregion");
		}

		private void WriteStructs(TextWriter writer)
		{
			Log?.LogVerbose("  Writing structs...");

			writer.WriteLine($"\t#region Structs");

			var classes = compilation.Classes
				.Where(c => c.SizeOf != 0)
				.OrderBy(c => c.GetDisplayName())
				.ToList();
			foreach (var klass in classes)
			{
				Log?.LogVerbose($"    {klass.GetDisplayName()}");

				var name = klass.GetDisplayName();
				typeMappings.TryGetValue(name, out var map);
				name = map?.CsType ?? Utils.CleanName(name);

				writer.WriteLine();
				writer.WriteLine($"\t// {klass.GetDisplayName()}");
				writer.WriteLine($"\t[StructLayout (LayoutKind.Sequential)]");
				var visibility = "public";
				if (map?.IsInternal == true)
					visibility = "internal";
				writer.WriteLine($"\t{visibility} unsafe partial struct {name} {{");
				foreach (var field in klass.Fields)
				{
					var type = GetType(field.Type);
					var cppT = GetCppType(field.Type);

					writer.WriteLine($"\t\t// {field}");

					var fieldName = field.Name;
					var isPrivate = fieldName.StartsWith("_private_", StringComparison.OrdinalIgnoreCase);
					if (isPrivate) {
						fieldName = fieldName.Substring(9);
					}

					var vis = "private";
					if (map?.IsInternal == true)
						vis = "public";
					writer.WriteLine($"\t\t{vis} {type} {fieldName};");

					if (!isPrivate && (map == null || (map.GenerateProperties && !map.IsInternal)))
					{
						var propertyName = fieldName;
						if (map != null && map.Members.TryGetValue(propertyName, out var fieldMap))
							propertyName = fieldMap;
						else
							propertyName = Utils.CleanName(propertyName);

						if (cppT == "bool")
						{
							writer.WriteLine($"\t\tpublic bool {propertyName} {{");
							writer.WriteLine($"\t\t\tget => {fieldName} > 0;");
							writer.WriteLine($"\t\t\tset => {fieldName} = value ? (byte)1 : (byte)0;");
							writer.WriteLine($"\t\t}}");
						}
						else
						{
							writer.WriteLine($"\t\tpublic {type} {propertyName} {{");
							writer.WriteLine($"\t\t\tget => {fieldName};");
							writer.WriteLine($"\t\t\tset => {fieldName} = value;");
							writer.WriteLine($"\t\t}}");
						}
						writer.WriteLine();
					}
				}
				writer.WriteLine($"\t}}");
			}

			writer.WriteLine();
			writer.WriteLine($"\t#endregion");
		}

		private void WriteEnums(TextWriter writer)
		{
			Log?.LogVerbose("  Writing enums...");

			writer.WriteLine($"\t#region Enums");

			var enums = compilation.Enums
				.OrderBy(c => c.GetDisplayName())
				.ToList();
			foreach (var enm in enums)
			{
				Log?.LogVerbose($"    {enm.GetDisplayName()}");

				var name = enm.GetDisplayName();
				typeMappings.TryGetValue(name, out var map);
				name = map?.CsType ?? Utils.CleanName(name);

				var visibility = "public";
				if (map?.IsInternal == true)
					visibility = "internal";

				writer.WriteLine();
				writer.WriteLine($"\t// {enm.GetDisplayName()}");
				if (map?.IsFlags == true)
					writer.WriteLine($"\t[Flags]");
				writer.WriteLine($"\t{visibility} enum {name} {{");
				foreach (var field in enm.Items)
				{
					var fieldName = field.Name;
					if (map != null && map.Members.TryGetValue(fieldName, out var fieldMap))
						fieldName = fieldMap;
					else
						fieldName = Utils.CleanName(fieldName, isEnumMember: true);

					writer.WriteLine($"\t\t// {field.Name} = {field.ValueExpression?.ToString() ?? field.Value.ToString()}");
					writer.WriteLine($"\t\t{fieldName} = {field.Value},");
				}
				writer.WriteLine($"\t}}");
			}

			writer.WriteLine();
			writer.WriteLine($"\t#endregion");
		}

		private void WriteClasses(TextWriter writer)
		{
			Log?.LogVerbose("  Writing usings...");

			writer.WriteLine($"\t#region Class declarations");
			writer.WriteLine();

			var classes = compilation.Classes
				.OrderBy(c => c.GetDisplayName())
				.ToList();
			foreach (var klass in classes)
			{
				var type = klass.GetDisplayName();
				skiaTypes.Add(type, klass.SizeOf != 0);

				if (klass.SizeOf == 0)
					writer.WriteLine($"\tusing {klass.GetDisplayName()} = IntPtr;");

				Log?.LogVerbose($"    {klass.GetDisplayName()}");
			}

			writer.WriteLine();
			writer.WriteLine($"\t#endregion");
		}

		private void WriteFunctions(TextWriter writer)
		{
			Log?.LogVerbose("  Writing p/invokes...");

			var functionGroups = compilation.Functions
				.OrderBy(f => f.Name)
				.GroupBy(f => f.Span.Start.File);
			foreach (var group in functionGroups)
			{
				writer.WriteLine($"\t\t#region {Path.GetFileName(group.Key)}");
				foreach (var function in group)
				{
					Log?.LogVerbose($"    {function.Name}");

					writer.WriteLine();
					writer.WriteLine($"\t\t// {function}");
					writer.WriteLine($"\t\t[DllImport ({config.DllName}, CallingConvention = CallingConvention.Cdecl)]");

					var name = function.Name;
					functionMappings.TryGetValue(name, out var funcMap);

					var paramsList = new List<string>();
					for (var i = 0; i < function.Parameters.Count; i++)
					{
						var p = function.Parameters[i];
						var n = string.IsNullOrEmpty(p.Name) ? $"param{i}" : p.Name;
						var t = GetType(p.Type);
						var cppT = GetCppType(p.Type);
						if (cppT == "bool")
							t = $"[MarshalAs (UnmanagedType.I1)] bool";
						if (funcMap != null && funcMap.Parameters.TryGetValue(i.ToString(), out var newT))
							t = newT;
						paramsList.Add($"{t} {n}");
					}

					var returnType = GetType(function.ReturnType);
					if (funcMap != null && funcMap.Parameters.TryGetValue("-1", out var newR))
					{
						returnType = newR;
					}
					else if (GetCppType(function.ReturnType) == "bool")
					{
						returnType = "bool";
						writer.WriteLine($"\t\t[return: MarshalAs (UnmanagedType.I1)]");
					}
					writer.WriteLine($"\t\tinternal static extern {returnType} {name} ({string.Join(", ", paramsList)});");
				}
				writer.WriteLine();
				writer.WriteLine($"\t\t#endregion");
				writer.WriteLine();
			}
		}

		private string GetType(CppType type)
		{
			var typeName = GetCppType(type);

			// split the type from the pointers
			var pointerIndex = typeName.IndexOf("*");
			var pointers = pointerIndex == -1 ? "" : typeName.Substring(pointerIndex);
			var noPointers = pointerIndex == -1 ? typeName : typeName.Substring(0, pointerIndex);

			if (skiaTypes.TryGetValue(noPointers, out var isStruct))
			{
				if (!isStruct)
					return noPointers + pointers.Substring(1);
				if (typeMappings.TryGetValue(noPointers, out var map))
					return (map.CsType ?? Utils.CleanName(noPointers)) + pointers;
			}
			else
			{
				if (typeMappings.TryGetValue(typeName, out var map))
					return map.CsType ?? Utils.CleanName(typeName);
				if (typeMappings.TryGetValue(noPointers, out map))
					return (map.CsType ?? Utils.CleanName(noPointers)) + pointers;
				if (functionMappings.TryGetValue(typeName, out var funcMap))
					return funcMap.CsType ?? Utils.CleanName(typeName);
				if (functionMappings.TryGetValue(noPointers, out funcMap))
					return (funcMap.CsType ?? Utils.CleanName(noPointers)) + pointers;
			}

			return Utils.CleanName(typeName);
		}

		private static string GetCppType(CppType type)
		{
			var typeName = type.GetDisplayName();

			// remove the const
			typeName = typeName.Replace("const ", "");

			// replace the [] with a *
			int start;
			while ((start = typeName.IndexOf("[")) != -1)
			{
				var end = typeName.IndexOf("]");
				typeName = typeName.Substring(0, start) + "*" + typeName.Substring(end + 1);
			}

			return typeName;
		}
	}
}
