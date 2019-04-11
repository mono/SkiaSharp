using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Uno.RoslynHelpers;
using Uno.SourceGeneration;

namespace SkiaSharp.Wasm.Generator
{
	[Uno.SourceGeneration.GenerateAfter("SkiaSharp.Wasm.Generator.SkiaApiGenerator")]
	public class TSBindingsGenerator : SourceGenerator
	{
		private string _bindingsPaths;
		private string[] _sourceAssemblies;

		private static INamedTypeSymbol _stringSymbol;
		private static INamedTypeSymbol _intSymbol;
		private static INamedTypeSymbol _floatSymbol;
		private static INamedTypeSymbol _doubleSymbol;
		private static INamedTypeSymbol _byteSymbol;
		private static INamedTypeSymbol _sbyteSymbol;
		private static INamedTypeSymbol _shortSymbol;
		private static INamedTypeSymbol _ushortSymbol;
		private static INamedTypeSymbol _intPtrSymbol;
		private static INamedTypeSymbol _uintSymbol;
		private static INamedTypeSymbol _boolSymbol;
		private static INamedTypeSymbol _longSymbol;
		private static INamedTypeSymbol _structLayoutSymbol;
		private static INamedTypeSymbol _interopMessageSymbol;

		private Dictionary<ITypeSymbol, int> _structSize
			= new Dictionary<ITypeSymbol, int> ();

		public override void Execute(SourceGeneratorContext context)
		{
			var project = context.GetProjectInstance();
			_bindingsPaths = project.GetPropertyValue("TSBindingsPath")?.ToString();
			_sourceAssemblies = project.GetItems("TSBindingAssemblySource").Select(s => s.EvaluatedInclude).ToArray();

			if(!string.IsNullOrEmpty(_bindingsPaths))
			{
				_stringSymbol = context.Compilation.GetTypeByMetadataName("System.String");
				_intSymbol = context.Compilation.GetTypeByMetadataName ("System.Int32");
				_floatSymbol = context.Compilation.GetTypeByMetadataName("System.Single");
				_doubleSymbol = context.Compilation.GetTypeByMetadataName("System.Double");
				_byteSymbol = context.Compilation.GetTypeByMetadataName ("System.Byte");
				_sbyteSymbol = context.Compilation.GetTypeByMetadataName ("System.SByte");
				_shortSymbol = context.Compilation.GetTypeByMetadataName ("System.Int16");
				_ushortSymbol = context.Compilation.GetTypeByMetadataName ("System.UInt16");
				_intPtrSymbol = context.Compilation.GetTypeByMetadataName("System.IntPtr");
				_uintSymbol = context.Compilation.GetTypeByMetadataName("System.UInt32");
				_boolSymbol = context.Compilation.GetTypeByMetadataName("System.Boolean"); 
				_longSymbol = context.Compilation.GetTypeByMetadataName("System.Int64");
				_structLayoutSymbol = context.Compilation.GetTypeByMetadataName(typeof(System.Runtime.InteropServices.StructLayoutAttribute).FullName);
				_interopMessageSymbol = context.Compilation.GetTypeByMetadataName("SkiaSharp.TSInteropMessageAttribute");
				 
				var modules = from ext in context.Compilation.ExternalReferences
							  let sym = context.Compilation.GetAssemblyOrModuleSymbol(ext) as IAssemblySymbol
							  where _sourceAssemblies.Contains(sym.Name)
							  from module in sym.Modules
							  select module;

				modules = modules.Concat(new[] { context.Compilation.SourceModule }).ToArray();

				GenerateTSMarshallingLayouts(modules);
			}
		}

		internal void GenerateTSMarshallingLayouts(IEnumerable<IModuleSymbol> modules)
		{
			var messageTypes = from module in modules
							   from type in GetNamespaceTypes(module)
							   where (
								   type.FindAttributeFlattened(_interopMessageSymbol) != null
								   && type.TypeKind == TypeKind.Struct
							   )
							   select type;

			messageTypes = messageTypes.ToArray();

			foreach (var messageType in messageTypes) {
				GenerateForType (messageType);
			}
		}

		private void GenerateForType (INamedTypeSymbol messageType)
		{
			var packValue = GetStructPack (messageType);

			if(packValue == 0) {
				packValue = 4;
			}

			var sb = new IndentedStringBuilder ();

			sb.AppendLineInvariant ($"/* {nameof (TSBindingsGenerator)} Generated code -- this code is regenerated on each build */");

			var structSize = 0;

			using (sb.BlockInvariant ($"namespace {messageType.ContainingNamespace.Name}")) {
				using (sb.BlockInvariant ($"export class {messageType.Name}")) {
					sb.AppendLineInvariant ($"/* Pack={packValue} */");

					foreach (var field in messageType.GetFields ().Where (f => !f.IsStatic)) {
						TryGenerateForType (field.Type);

						sb.AppendLineInvariant ($"{field.Name} : {GetTSFieldType (field.Type)};");

						structSize += GetNativeFieldSize (field);
					}

					if (messageType.Name.EndsWith ("Params") || (!messageType.Name.EndsWith ("Return") && !messageType.Name.EndsWith ("Params"))) {
						GenerateUmarshaler (messageType, sb, packValue);
					}

					if (messageType.Name.EndsWith ("Return") || (!messageType.Name.EndsWith ("Return") && !messageType.Name.EndsWith ("Params"))) {

						using (sb.BlockInvariant ($"public constructor()")) {
							foreach (var field in messageType.GetFields ().Where (f => !f.IsStatic && _structSize.ContainsKey(f.Type))) {
								sb.AppendLineInvariant ($"this.{field.Name} = new {GetTSFieldType (field.Type)}();");
							}
						}

						GenerateMarshaler (messageType, sb, packValue);
					}
				}
			}

			var outputPath = Path.Combine (_bindingsPaths, $"{messageType.Name}.ts");

			File.WriteAllText (outputPath, sb.ToString ());

			_structSize[messageType] = structSize;
		}

		private void TryGenerateForType (ITypeSymbol type)
		{
			if (type.TypeKind == TypeKind.Struct && type.ContainingNamespace.Name == "SkiaSharp") {
				GenerateForType (type as INamedTypeSymbol);
			}
			if (type is IArrayTypeSymbol array) {
				TryGenerateForType (array.ElementType);
			}
		}

		private static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(IModuleSymbol module)
		{
			foreach(var type in module.GlobalNamespace.GetNamespaceTypes())
			{
				yield return type;

				foreach(var inner in type.GetTypeMembers())
				{
					yield return inner;
				}
			}
		}

		private int GetStructPack(INamedTypeSymbol parametersType)
		{
			// https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/Symbols/TypeLayout.cs is not available.

			if (parametersType.GetType().GetProperty("Layout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is PropertyInfo info)
			{
				if (info.GetValue(parametersType) is object typeLayout)
				{
					if (typeLayout.GetType().GetProperty("Kind", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is PropertyInfo layoutKingProperty)
					{
						if (((System.Runtime.InteropServices.LayoutKind)layoutKingProperty.GetValue(typeLayout)) == System.Runtime.InteropServices.LayoutKind.Sequential)
						{
							if (typeLayout.GetType().GetProperty("Alignment", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is PropertyInfo alignmentProperty)
							{
								return (short)alignmentProperty.GetValue(typeLayout);
							}
						}
						else
						{
							throw new InvalidOperationException($"The LayoutKind for {parametersType} must be LayoutKind.Sequential");
						}
					}
				}
			}

			throw new InvalidOperationException($"Failed to get structure layout, unknown roslyn internal structure");
		}

		private bool IsMarshalledExplicitly(IFieldSymbol fieldSymbol)
		{
			// https://github.com/dotnet/roslyn/blob/0610c79807fa59d0815f2b89e5283cf6d630b71e/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PEFieldSymbol.cs#L133 is not available.

			if (fieldSymbol.GetType().GetProperty(
				"IsMarshalledExplicitly",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is PropertyInfo info
			)
			{
				if (info.GetValue(fieldSymbol) is bool isMarshalledExplicitly)
				{
					return isMarshalledExplicitly;
				}
			}

			throw new InvalidOperationException($"Failed to IsMarshalledExplicitly, unknown roslyn internal structure");
		}

		private void GenerateMarshaler(INamedTypeSymbol parametersType, IndentedStringBuilder sb, int packValue)
		{
			var structSize = parametersType.GetFields ().Where (f => !f.IsStatic).Select (GetNativeFieldSize).Sum ();

			using (sb.BlockInvariant ($"public marshalNew(memoryContext: any = null) : number")) {
				sb.AppendLineInvariant ($"memoryContext = memoryContext ? memoryContext : Module;");
				sb.AppendLineInvariant ($"var pTarget = memoryContext._malloc({structSize});");

				sb.AppendLineInvariant ($"this.marshal(pTarget, memoryContext);");

				sb.AppendLineInvariant ($"return pTarget;");
			}

			using (sb.BlockInvariant($"public marshal(pData:number, memoryContext: any = null)"))
			{
				sb.AppendLineInvariant($"memoryContext = memoryContext ? memoryContext : Module;");

				var fieldOffset = 0;

				foreach (var field in parametersType.GetFields().Where (f => !f.IsStatic))
				{
					var fieldSize = GetNativeFieldSize(field);
					bool isStringField = field.Type == _stringSymbol;

					if (field.Type is IArrayTypeSymbol arraySymbol)
					{
						throw new NotSupportedException($"Return value array fields are not supported ({field})");
					}
					else
					{
						var value = $"this.{field.Name}";

						if (isStringField)
						{
							using (sb.BlockInvariant(""))
							{
								sb.AppendLineInvariant($"var stringLength = lengthBytesUTF8({value});");
								sb.AppendLineInvariant($"var pString = memoryContext._malloc(stringLength + 1);");
								sb.AppendLineInvariant($"stringToUTF8({value}, pString, stringLength + 1);");

								sb.AppendLineInvariant(
									$"memoryContext.setValue(pData + {fieldOffset}, pString, \"*\");"
								);
							}
						} else if (_structSize.ContainsKey (field.Type)) {
							sb.AppendLineInvariant ($"{value}.marshal(pData + {fieldOffset});");
						} else {
							sb.AppendLineInvariant(
								$"memoryContext.setValue(pData + {fieldOffset}, {value}, \"{GetEMField(field.Type)}\");"
							);
						}
					}

					fieldOffset += fieldSize;

					var adjust = fieldOffset % packValue;
					if (adjust != 0)
					{
						fieldOffset += (packValue - adjust);
					}
				}
			}
		}

		private void GenerateUmarshaler(INamedTypeSymbol parametersType, IndentedStringBuilder sb, int packValue)
		{
			using (sb.BlockInvariant($"public static unmarshal(pData:number, memoryContext: any = null) : {parametersType.Name}"))
			{
				sb.AppendLineInvariant ($"memoryContext = memoryContext ? memoryContext : Module;");

				sb.AppendLineInvariant($"let ret = new {parametersType.Name}();");

				var fieldOffset = 0;
				foreach (var field in parametersType.GetFields().Where (f => !f.IsStatic))
				{
					var fieldSize = GetNativeFieldSize(field);

					if (field.Type is IArrayTypeSymbol arraySymbol)
					{
						if (!IsMarshalledExplicitly(field))
						{
							// This is required by the mono-wasm AOT engine for fields to be properly considered.
							throw new InvalidOperationException($"The field {field} is an array but does not have a [MarshalAs(UnmanagedType.LPArray)] attribute");
						}

						var elementType = arraySymbol.ElementType;
						var elementTSType = GetTSType(elementType);
						var isElementString = elementType == _stringSymbol;
						var elementSize = GetNativeElementSize (elementType);

						using (sb.BlockInvariant(""))
						{
							sb.AppendLineInvariant($"var pArray = memoryContext.getValue(pData + {fieldOffset}, \"*\"); /*{elementType} {elementSize} {isElementString}*/");

							using (sb.BlockInvariant("if(pArray !== 0)"))
							{
								sb.AppendLineInvariant($"ret.{field.Name} = new Array<{GetTSFieldType(elementType)}>();");

								using (sb.BlockInvariant($"for(var i=0; i<ret.{field.Name}_Length; i++)"))
								{
									if (_structSize.ContainsKey (elementType)) {
										sb.AppendLineInvariant ($"ret.{field.Name}.push({elementType}.unmarshal(pArray + i * {elementSize}));");
									} else {
										sb.AppendLineInvariant ($"var value = memoryContext.getValue(pArray + i * {elementSize}, \"{GetEMField (elementType)}\");");

										if (isElementString) {
											using (sb.BlockInvariant ("if(value !== 0)")) {
												sb.AppendLineInvariant ($"ret.{field.Name}.push({elementTSType}(MonoRuntime.conv_string(value)));");
											}
											sb.AppendLineInvariant ("else");
											using (sb.BlockInvariant ("")) {
												sb.AppendLineInvariant ($"ret.{field.Name}.push(null);");
											}
										} else {
											sb.AppendLineInvariant ($"ret.{field.Name}.push({elementTSType}(value));");

										}
									}
								}
							}
							sb.AppendLineInvariant("else");
							using (sb.BlockInvariant(""))
							{
								sb.AppendLineInvariant($"ret.{field.Name} = null;");
							}
						}
					}
					else
					{
						using (sb.BlockInvariant(""))
						{
							if(field.Type == _stringSymbol)
							{
								sb.AppendLineInvariant($"var ptr = memoryContext.getValue(pData + {fieldOffset}, \"{GetEMField(field.Type)}\");");

								using (sb.BlockInvariant("if(ptr !== 0)"))
								{
									sb.AppendLineInvariant($"ret.{field.Name} = {GetTSType(field.Type)}(memoryContext.UTF8ToString(ptr));");
								}
								sb.AppendLineInvariant("else");
								using (sb.BlockInvariant(""))
								{
									sb.AppendLineInvariant($"ret.{field.Name} = null;");
								}
							}
							else if (_structSize.ContainsKey(field.Type)) {
								sb.AppendLineInvariant($"ret.{field.Name} = {field.Type}.unmarshal(pData + {fieldOffset});");
							}
							else {
								sb.AppendLineInvariant($"ret.{field.Name} = {GetTSType(field.Type)}(memoryContext.getValue(pData + {fieldOffset}, \"{GetEMField(field.Type)}\"));");
							}
						}
					}

					fieldOffset += fieldSize;

					var adjust = fieldOffset % packValue;
					if (adjust != 0)
					{
						fieldOffset += (packValue - adjust);
					}
				}

				sb.AppendLineInvariant($"return ret;");
			}
		}

		private int GetNativeFieldSize(IFieldSymbol field)
		{
			if(
				field.Type == _stringSymbol
				|| field.Type == _intSymbol
				|| field.Type == _uintSymbol
				|| field.Type == _byteSymbol
				|| field.Type == _shortSymbol
				|| field.Type == _ushortSymbol
				|| field.Type == _sbyteSymbol
				|| field.Type == _intPtrSymbol
				|| field.Type == _floatSymbol
				|| field.Type == _boolSymbol
				|| field.Type is IArrayTypeSymbol
				|| field.Type.TypeKind == TypeKind.Enum
				|| field.Type.TypeKind == TypeKind.Pointer
			)
			{
				return 4;
			}
			else if(field.Type == _doubleSymbol || field.Type == _longSymbol)
			{
				return 8;
			}
			else if(_structSize.TryGetValue(field.Type, out var size)) {
				return size;
			}
			else
			{
				throw new NotSupportedException($"The field [{field} {field.Type}] is not supported");
			}
		}

		private int GetNativeElementSize (ITypeSymbol type)
		{
			switch (type.ToString ()) {
				case "bool":
				case "int":
				case "uint":
				case "System.IntPtr":
				case "float":
				case var _ when type.TypeKind == TypeKind.Enum:
					return 4;

				case "double":
				case "long":
					return 8;

				case "short":
				case "ushort":
					return 2;

				case "sbyte":
				case "byte":
					return 1;

				default:
					return 4; 
					// throw new NotSupportedException ($"GetNativeElementSize for {type} is not supported");
			}
		}

		private static string GetEMField(ITypeSymbol fieldType)
		{
			if (
				fieldType == _stringSymbol
				|| fieldType == _intPtrSymbol
				|| fieldType is IArrayTypeSymbol
				|| fieldType.TypeKind == TypeKind.Pointer
			)
			{
				return "*";
			}
			else if (
				fieldType == _intSymbol
                || fieldType == _uintSymbol
				|| fieldType == _boolSymbol
                || fieldType.TypeKind == TypeKind.Enum
            )
			{
				return "i32";
			}
			else if (fieldType == _longSymbol)
			{
				return "i64";
			}
			else if (fieldType == _shortSymbol || fieldType == _ushortSymbol)
			{
				return "i16";
			}
			else if (fieldType == _byteSymbol || fieldType == _sbyteSymbol)
			{
				return "i8";
			}
			else if (fieldType == _floatSymbol)
			{
				return "float";
			}
			else if (fieldType == _doubleSymbol)
			{
				return "double";
			}
			else
			{
				throw new NotSupportedException($"Unsupported EM type conversion [{fieldType}]");
			}
		}

		private string GetTSType(ITypeSymbol type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type is IArrayTypeSymbol array)
			{
				return $"Array<{GetTSType(array.ElementType)}>";
			}
			else if (type == _stringSymbol)
			{
				return "String";
			}
			else if (
				type == _intSymbol
				|| type == _uintSymbol
				|| type == _floatSymbol
				|| type == _longSymbol
				|| type == _doubleSymbol
				|| type == _byteSymbol
				|| type == _sbyteSymbol
				|| type == _shortSymbol
				|| type == _ushortSymbol
				|| type == _intPtrSymbol
				|| type.TypeKind == TypeKind.Enum
				|| type.TypeKind == TypeKind.Pointer
			)
			{
				return "Number";
			}
			else if (type == _boolSymbol)
			{
				return "Boolean";
            }
            else if (_structSize.ContainsKey(type))
            {
                return type.ToDisplayString();
            }
            else
            {
				throw new NotSupportedException($"The type {type} is not supported");
			}
		}

		private string GetTSFieldType(ITypeSymbol type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type is IArrayTypeSymbol array)
			{
				return $"Array<{GetTSFieldType(array.ElementType)}>";
			}
			else if (type == _stringSymbol)
			{
				return "string";
			}
			else if (
				type == _intSymbol
				|| type == _uintSymbol
				|| type == _longSymbol
				|| type == _floatSymbol
				|| type == _doubleSymbol
				|| type == _byteSymbol
				|| type == _sbyteSymbol
				|| type == _shortSymbol
				|| type == _ushortSymbol
				|| type == _intPtrSymbol
                || type.TypeKind == TypeKind.Enum
                || type.TypeKind == TypeKind.Pointer
            )
			{
				return "number";
			}
            else if (type == _boolSymbol)
            {
                return "boolean";
			}
			else if (_structSize.ContainsKey (type)) {
				return type.ToDisplayString ();
			} else
            {
				throw new NotSupportedException($"The type {type} is not supported");
			}
		}

	}
}
