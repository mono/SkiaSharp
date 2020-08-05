using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CppAst;

namespace SkiaSharpGenerator
{
	public abstract class BaseTool
	{
		protected readonly Dictionary<string, TypeMapping> typeMappings = new Dictionary<string, TypeMapping>();
		protected readonly Dictionary<string, FunctionMapping> functionMappings = new Dictionary<string, FunctionMapping>();
		protected readonly Dictionary<string, bool> skiaTypes = new Dictionary<string, bool>();

		protected readonly List<string> excludedFiles = new List<string>();
		protected readonly List<string> excludedTypes = new List<string>();

		protected CppCompilation compilation = new CppCompilation();
		protected Config config = new Config();

		protected BaseTool(string skiaRoot, string configFile)
		{
			SkiaRoot = skiaRoot ?? throw new ArgumentNullException(nameof(skiaRoot));
			ConfigFile = configFile ?? throw new ArgumentNullException(nameof(configFile));
		}

		public ILogger? Log { get; set; }

		public string SkiaRoot { get; }

		public string ConfigFile { get; }

		public bool HasErrors =>
			compilation?.Diagnostics?.HasErrors ?? false;

		public IEnumerable<CppDiagnosticMessage> Messages =>
			compilation?.Diagnostics?.Messages ?? Array.Empty<CppDiagnosticMessage>();

		protected void ParseSkiaHeaders()
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

			foreach (var filter in config.Exclude.Files)
			{
				excludedFiles.AddRange(Directory.EnumerateFiles(SkiaRoot, filter));
			}

			foreach (var filter in config.Exclude.Types)
			{
				excludedTypes.Add(filter);
				excludedTypes.Add(filter + "*");
				excludedTypes.Add(filter + "**");
			}

			foreach (var f in excludedFiles)
				Log?.LogVerbose("Skipping everything in: " + f);

			compilation = CppParser.ParseFiles(headers, options);

			if (compilation == null || compilation.HasErrors)
			{
				Log?.LogError("Parsing headers failed.");
				throw new Exception("Parsing headers failed.");
			}
		}

		protected void LoadStandardMappings()
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

		protected void UpdatingMappings()
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

		protected async Task<Config> LoadConfigAsync(string configPath)
		{
			Log?.LogVerbose("Loading configuration...");

			using var configJson = File.OpenRead(configPath);

			return await JsonSerializer.DeserializeAsync<Config>(configJson, new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
			});
		}

		protected string GetType(CppType type)
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

		protected static string GetCppType(CppType type)
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
