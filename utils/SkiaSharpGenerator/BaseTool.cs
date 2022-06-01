using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CppAst;

namespace SkiaSharpGenerator
{
	public abstract class BaseTool
	{
		private static readonly string[] keywords =
		{
			"out", "in", "var", "ref"
		};

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
				{ "long long",            nameof(Int64) },
				{ "long long int",        nameof(Int64) },
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

			var config = await JsonSerializer.DeserializeAsync<Config>(configJson, new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
			});

			return config ?? throw new InvalidOperationException("Unable to parse json config file.");
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
					return (map.CsType ?? CleanName(noPointers)) + pointers;
			}
			else
			{
				if (typeMappings.TryGetValue(typeName, out var map))
					return map.CsType ?? CleanName(typeName);
				if (typeMappings.TryGetValue(noPointers, out map))
					return (map.CsType ?? CleanName(noPointers)) + pointers;
				if (functionMappings.TryGetValue(typeName, out var funcMap))
					return funcMap.CsType ?? CleanName(typeName);
				if (functionMappings.TryGetValue(noPointers, out funcMap))
					return (funcMap.CsType ?? CleanName(noPointers)) + pointers;
			}

			return CleanName(typeName);
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
				typeName = typeName[..start] + "*" + typeName[(end + 1)..];
			}

			return typeName;
		}

		protected string CleanName(string type)
		{
			var prefix = "";
			var suffix = "";

			type = type.TrimStart('_');

			if (type.EndsWith("_t"))
				type = type[0..^2];

			if (type.EndsWith("_proc") || type.EndsWith("_func"))
			{
				type = type[0..^5];
				suffix = "ProxyDelegate";
			}

			foreach (var ns in config.Namespaces)
			{
				var nsPrefix = ns.Key;
				if (type.StartsWith(nsPrefix))
				{
					var mapping = ns.Value;
					if (mapping.Prefix != null)
					{
						prefix = mapping.Prefix;
						type = type[nsPrefix.Length..];
					}
				}
			}

			string[] parts;

			if (type.Any(c => char.IsLower(c)) && type.Any(c => char.IsUpper(c)))
			{
				// there is a mix of cases, so split on the uppercase, then underscores
				parts = Regex.Split(type, @"(?<!^)(?=[A-Z])");
				parts = parts.SelectMany(p => p.Split('_')).ToArray();

				// remove the initial "f" prefix
				if (parts[0] == "f" && char.IsUpper(parts[1][0]))
					parts = parts[1..];
			}
			else
			{
				// this is either an fully uppercase enum or lowercase type
				parts = type.ToLowerInvariant().Split('_');
			}

			for (var i = 0; i < parts.Length; i++)
			{
				var part = parts[i];
				parts[i] = part[0].ToString().ToUpperInvariant() + part[1..];
			}

			return prefix + string.Concat(parts) + suffix;
		}

		protected string CleanEnumFieldName(string fieldName, string cppEnumName)
		{
			if (cppEnumName.EndsWith("_t"))
				cppEnumName = cppEnumName[..^2];

			fieldName = RemovePrefixSuffix(fieldName, cppEnumName);

			// special case for "flags" name and "flag" member
			if (cppEnumName.EndsWith("_flags"))
				fieldName = RemovePrefixSuffix(fieldName, cppEnumName[..^1]);

			// special case for bad skia enum fields
			var lower = fieldName.ToLowerInvariant();
			var indexOfSplitter = lower.IndexOf("_sk_");
			if (indexOfSplitter == -1)
				indexOfSplitter = lower.IndexOf("_gr_");
			if (indexOfSplitter != -1)
				fieldName = fieldName[0..indexOfSplitter];

			return CleanName(fieldName);

			static string RemovePrefixSuffix(string member, string type)
			{
				if (member.ToLowerInvariant().EndsWith("_" + type.ToLowerInvariant()))
					member = member[..^(type.Length + 1)];

				if (member.ToLowerInvariant().StartsWith(type.ToLowerInvariant() + "_"))
					member = member[(type.Length + 1)..];

				return member;
			}
		}

		protected bool IncludeNamespace(string name)
		{
			foreach (var ns in config.Namespaces)
			{
				if (name.StartsWith(ns.Key) && ns.Value.Exclude == true)
					return false;
			}

			return true;
		}

		protected string GetNamespace(string name)
		{
			foreach (var ns in config.Namespaces)
			{
				if (name.StartsWith(ns.Key) && !string.IsNullOrWhiteSpace(ns.Value.CsName))
					return $"{config.Namespace}.{ns.Value.CsName}";
			}

			return config.Namespace;
		}

		protected static string SafeName(string name) =>
			keywords.Contains(name) ? "@" + name : name;
	}
}
