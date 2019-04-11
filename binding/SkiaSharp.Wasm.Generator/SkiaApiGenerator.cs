using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Uno.RoslynHelpers;
using Uno.SourceGeneration;

namespace SkiaSharp.Wasm.Generator
{
	public class SkiaApiGenerator : SourceGenerator
	{
		private string _bindingsPaths;
		private string[] _sourceAssemblies;
		private INamedTypeSymbol _skColorSymbol;
		private INamedTypeSymbol _stringSymbol;
		private INamedTypeSymbol _byteSymbol;
		private INamedTypeSymbol _floatSymbol;
		private INamedTypeSymbol _intPtrSymbol;
		private INamedTypeSymbol _uintSymbol;
		private INamedTypeSymbol _ushortSymbol;

		public override void Execute (SourceGeneratorContext context)
		{
			var project = context.GetProjectInstance ();
			_bindingsPaths = project.GetPropertyValue ("TSBindingsPath")?.ToString ();
			_sourceAssemblies = project.GetItems ("TSBindingAssemblySource").Select (s => s.EvaluatedInclude).ToArray ();

			var skiaApiFile = Path.Combine (context.GetProjectInstance ().Directory, "..", "Binding", "SkiaApi.cs");

			var skiaTree = SyntaxFactory.ParseCompilationUnit (File.ReadAllText (skiaApiFile).Replace ("SkiaApi", "OriginalSkiaApi"));

			var compilation = context.Compilation.AddSyntaxTrees (skiaTree.SyntaxTree);

			var symbol = compilation.GetTypeByMetadataName ("SkiaSharp.OriginalSkiaApi");
			_skColorSymbol = compilation.GetTypeByMetadataName ("SkiaSharp.SKColor");
			_stringSymbol = compilation.GetTypeByMetadataName ("System.String");
			_byteSymbol = compilation.GetTypeByMetadataName ("System.Byte");
			_floatSymbol = compilation.GetTypeByMetadataName ("System.Single");
			_intPtrSymbol = compilation.GetTypeByMetadataName ("System.IntPtr");
			_uintSymbol = compilation.GetTypeByMetadataName ("System.UInt32");
			_ushortSymbol = compilation.GetTypeByMetadataName ("System.UInt16");

			BuildParamsStructs (context, skiaApiFile, compilation, symbol);
			BuildTSInvokers (context, symbol);
		}

		private void BuildTSInvokers (SourceGeneratorContext context, INamedTypeSymbol symbol)
		{
			var w = new IndentedStringBuilder ();

			using (w.BlockInvariant ($"namespace SkiaSharp")) {
				using (w.BlockInvariant ($"export class SkiaApi")) {

					foreach (var methodGroups in symbol.GetMethods ().GroupBy (g => g.Name)) {
						foreach (var method in methodGroups.Select ((m, i) => (m, i))) {

							using (w.BlockInvariant ($"public static {method.m.Name}_{method.i}(pParams : number, pReturn : number) : number")) {
								var paramsStructName = $"{method.m.Name}_{method.i}_Params";
								var returnStructName = $"{method.m.Name}_{method.i}_Return";


								var inputParams = from p in method.m.Parameters
											select p;

								var parmsString = string.Join (", ", inputParams.Select (p => p.Name));

								if (HasRefOrOutParams (method.m.Parameters)) {
									w.AppendLineInvariant ($"var retStruct = new {returnStructName}();");
								}

								if (inputParams.Any ()) {
									w.AppendLineInvariant ($"var parms = {paramsStructName}.unmarshal(pParams);");

									using (w.BlockInvariant ($"if((<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Pre)")) {
										w.AppendLineInvariant ($"(<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Pre(parms);");
									}

									string formatParam (IParameterSymbol p)
									{
										if(p.Type == _skColorSymbol) {
											return $"parms.{p.Name}.color";
										}
										else {
											var isSkiaType = p.Type.ContainingNamespace?.Name.StartsWith ("SkiaSharp") ?? false;

											if (p.Type.TypeKind == TypeKind.Struct && isSkiaType) {
												if (p.RefKind == RefKind.None || p.RefKind == RefKind.Ref) {
													return $"parms.{p.Name}.marshalNew(CanvasKit)";
												} else {
													return $"retStruct.{p.Name}.marshalNew(CanvasKit)";
												}
											} else {
												if (p.RefKind != RefKind.Out) {
													return "parms." + p.Name;
												} else {
													return $"CanvasKit._malloc({GetNativeSize (p.Type)})";
												}
											}
										}
									}

									foreach(var parm in inputParams){

										var paramString = formatParam (parm);
										if (!string.IsNullOrEmpty (paramString)) {

											if (
												parm.Type is IArrayTypeSymbol arraySymbol
												&& !(arraySymbol.ElementType.ContainingNamespace?.Name.StartsWith ("SkiaSharp") ?? false)
											) {
												if (arraySymbol.ElementType == _byteSymbol) {
													w.AppendLineInvariant ($"var {parm.Name} = CanvasKit._malloc(parms.{parm.Name}_Length * {GetNativeSize (arraySymbol.ElementType)}); /*{arraySymbol.ElementType}*/");

													using (w.BlockInvariant ($"")) {
														using (w.BlockInvariant ($"for(var i = 0; i < parms.{parm.Name}_Length; i++)")) {
															w.AppendLineInvariant ($"CanvasKit.HEAPU8[{parm.Name} + i] = parms.{parm.Name}[i];");
														}
													}
												}
												else if (arraySymbol.ElementType == _floatSymbol) {
													w.AppendLineInvariant ($"var {parm.Name} = CanvasKit._malloc(parms.{parm.Name}_Length * {GetNativeSize (arraySymbol.ElementType)}); /*{arraySymbol.ElementType}*/");
													w.AppendLineInvariant ($"var {parm.Name}_f32 = {parm.Name} / 4;");

													using (w.BlockInvariant ($"")) {
														using (w.BlockInvariant ($"for(var i = 0; i < parms.{parm.Name}_Length; i++)")) {
															w.AppendLineInvariant ($"CanvasKit.HEAPF32[{parm.Name}_f32 + i] = parms.{parm.Name}[i];");
														}
													}
												} else {
													w.AppendLineInvariant ($"var {parm.Name} = {paramString}; /* {arraySymbol.ElementType} */");
												}
											} else {
												w.AppendLineInvariant ($"var {parm.Name} = {paramString};");
											}
										}
									}
								}

								w.AppendLineInvariant ($"var ret = CanvasKit._{method.m.Name}({parmsString});");

								if (HasRefOrOutParams (method.m.Parameters)) {

									w.AppendLineInvariant ($"var retStruct = new {returnStructName}();");

									string formatParam (IParameterSymbol p)
									{
										if (p.Type.TypeKind == TypeKind.Struct
											&& (p.Type.ContainingNamespace?.Name.StartsWith ("SkiaSharp") ?? false)) {
											return $"{p.Type}.unmarshal({p.Name}, CanvasKit)";
										} else {
											return ReadNative(p);
										}
									}

									foreach (var parm in method.m.Parameters) {
										if (parm.RefKind != RefKind.None) {
											w.AppendLineInvariant ($"retStruct.{parm.Name} = {formatParam (parm)};");
										}
									}

									using (w.BlockInvariant ($"if((<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post)")) {
										w.AppendLineInvariant ($"ret = (<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post(ret, parms, retStruct);");
									}

									w.AppendLineInvariant ($"retStruct.marshal(pReturn);");
								} else if(inputParams.Any()) {
									using (w.BlockInvariant ($"if((<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post)")) {
										w.AppendLineInvariant ($"ret = (<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post(ret, parms);");
									}
								} else {
									using (w.BlockInvariant ($"if((<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post)")) {
										w.AppendLineInvariant ($"ret = (<any>SkiaSharp.ApiOverride).{method.m.Name}_{method.i}_Post(ret);");
									}
								}


								w.AppendLineInvariant ($"return ret;");
							}
						}
					}
				}
			}

			var outputPath = Path.Combine (_bindingsPaths, $"SkiaApi.ts");

			File.WriteAllText (outputPath, w.ToString ());
		}

		private string ReadNative (IParameterSymbol p)
		{
			switch (p.Type.ToString ()) {
				case "bool":
				case "int":
				case "uint":
				case "System.IntPtr":
				case var _ when p.Type.TypeKind == TypeKind.Enum:
					return $"CanvasKit.getValue({p.Name}, \"i32\")";

				case "float":
					return $"CanvasKit.getValue({p.Name}, \"float\")";

				case "double":
					return $"CanvasKit.getValue({p.Name}, \"double\")";

				case "long":
					return $"CanvasKit.getValue({p.Name}, \"i64\")";

				case "short":
				case "ushort":
					return $"CanvasKit.getValue({p.Name}, \"i16\")";

				case "sbyte":
				case "byte":
					return $"CanvasKit.getValue({p.Name}, \"i8\")";

				default:
					throw new NotSupportedException ($"ReadNative for {p.Type} in {p.ContainingSymbol} is not supported");
			}
		}

		private int GetNativeSize (ITypeSymbol type)
		{
			switch (type.ToString()) {
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
					throw new NotSupportedException ($"AllocNative for {type} is not supported");
			}
		}

		private void BuildParamsStructs (SourceGeneratorContext context, string skiaApiFile, Compilation compilation, INamedTypeSymbol symbol)
		{
			var w = new IndentedStringBuilder ();
			w.AppendLineInvariant ($"// {symbol} {skiaApiFile}");
			w.AppendLineInvariant ($"using System;");
			w.AppendLineInvariant ($"using System.Runtime.InteropServices;");

			using (w.BlockInvariant ("namespace SkiaSharp")) {
				using (w.BlockInvariant ("internal unsafe partial class SkiaApi")) {
					foreach (var methodGroups in symbol.GetMethods ().GroupBy (g => g.Name)) {
						foreach (var method in methodGroups.Select ((m, i) => (m, i))) {
							var parms = string.Join (", ", method.m.Parameters.Select (p => $"{FormatRefKind (p)} {p.Type} {p.Name}"));

							using (w.BlockInvariant ($"public static {method.m.ReturnType} {method.m.Name}({parms})")) {
								if (method.m.Parameters.Any ()) {

									var paramsStructName = $"{method.m.Name}_{method.i}_Params";
									var returnStructName = $"{method.m.Name}_{method.i}_Return";

									using (w.BlockInvariant ($"var _params = new {paramsStructName}")) {
										foreach (var parm in method.m.Parameters) {
											if (parm.RefKind != RefKind.Out) {
												w.AppendLineInvariant ($"{parm.Name} = {parm.Name},");
											}

											if(parm.Type.Kind== SymbolKind.ArrayType) {
												w.AppendLineInvariant ($"{parm.Name}_Length = {parm.Name}?.Length ?? 0,");
											}
										}
									}
									w.AppendLineInvariant ($";");

									if (HasRefOrOutParams (method.m.Parameters)) {
										w.AppendLineInvariant ($"var ret = SkiaSharp.TSInteropMarshaller.InvokeJS(\"Skia:{method.m.Name}_{method.i}\", _params, out {returnStructName} _retValues);");
									} else {
										w.AppendLineInvariant ($"var ret = SkiaSharp.TSInteropMarshaller.InvokeJS(\"Skia:{method.m.Name}_{method.i}\", _params);");
									}
								} else {
									w.AppendLineInvariant ($"var ret = SkiaSharp.WebAssemblyRuntime.InvokeJSUnmarshalled(\"Skia:{method.m.Name}_{method.i}\", System.IntPtr.Zero);");
								}

								if (HasRefOrOutParams (method.m.Parameters)) {
									foreach (var parm in method.m.Parameters) {
										if (parm.RefKind == RefKind.Out || parm.RefKind == RefKind.Ref) {
											w.AppendLineInvariant ($"{parm.Name} = _retValues.{parm.Name};");
										}
									}
								}

								if (method.m.ReturnType.ToString () == "System.IntPtr") {
									w.AppendLineInvariant ($"return ret;");
								} else if (method.m.ReturnType.ToString () == "bool") {
									w.AppendLineInvariant ($"return ret != System.IntPtr.Zero;");
								} else {

									if (method.m.ReturnType.ToString () != "void") {
										w.AppendLineInvariant ($"return ({method.m.ReturnType})ret;");
									}
								}
							}
						}
					}
				}

				foreach (var methodGroups in symbol.GetMethods ().GroupBy (g => g.Name)) {
					foreach (var method in methodGroups.Select ((m, i) => (m, i))) {
						if (method.m.Parameters.Any ()) {
							w.AppendLineInvariant ($"[SkiaSharp.TSInteropMessage]");
							w.AppendLineInvariant ($"[StructLayout(LayoutKind.Sequential, Pack = 4)]");
							using (w.BlockInvariant ($"internal unsafe struct {method.m.Name}_{method.i}_Params")) {
								foreach (var parm in method.m.Parameters) {
									if (parm.RefKind != RefKind.Out) {
										if (parm.Type.Kind == SymbolKind.ArrayType) {
											w.AppendLineInvariant ($"public int {parm.Name}_Length;");
											w.AppendLineInvariant ($"[MarshalAs(UnmanagedType.LPArray)]");
										}
										w.AppendLineInvariant ($"public {parm.Type} {parm.Name};");
									}
								}
							}

							if (HasRefOrOutParams (method.m.Parameters)) {
								w.AppendLineInvariant ($"[SkiaSharp.TSInteropMessage]");
								w.AppendLineInvariant ($"[StructLayout(LayoutKind.Sequential, Pack = 4)]");
								using (w.BlockInvariant ($"internal unsafe struct {method.m.Name}_{method.i}_Return")) {
									foreach (var parm in method.m.Parameters) {
										if (parm.RefKind == RefKind.Out || parm.RefKind == RefKind.Ref) {
											if (parm.Type.Kind == SymbolKind.ArrayType) {
												w.AppendLineInvariant ($"public int {parm.Name}_Length;");

												w.AppendLineInvariant ($"[MarshalAs(UnmanagedType.LPArray)]");
											}
											w.AppendLineInvariant ($"public {parm.Type} {parm.Name};");
										}
									}
								}
							}
						}
					}
				}
			}

			var includeFilters = new[] {
				"sk_canvas",
				"sk_surface",
				"sk_color",
				"sk_paint",
				"sk_typeface",
				"sk_string",
				"sk_shader",
				"sk_path",
				"sk_managed",
				"sk_codec",
				"sk_fontstyle",
				"sk_bitmap",
				"sk_imagefilter",
				"sk_matrix",
				"sk_maskfilter",
				"sk_vertices",
				"sk_3dview",
				"sk_filestream",
				"sk_memorystream",
				"sk_fontmgr",
			};

			var excludeFilters = new[] {
				"sk_colorspace_new_rgb_with_gamma_named_and_gamut"
			};

			var allExports = string.Join (",", symbol
				.GetMethods ()
				.GroupBy (g => g.Name)
				.Where (k => includeFilters.Any (k.Key.StartsWith))
				.Where (k => !excludeFilters.Any (k.Key.StartsWith))
				.Select (g => $"'_{g.Key}'")
			);

			w.AppendLineInvariant ($"// -s EXPORTED_FUNCTIONS=[{allExports}]");

			foreach (var diag in compilation.GetDiagnostics ()) {
				w.AppendLineInvariant ($"// {diag}");
			}
			foreach (var diag in compilation.GetDeclarationDiagnostics ()) {
				w.AppendLineInvariant ($"// {diag}");
			}

			context.AddCompilationUnit ("SkiaApi", w.ToString ());
		}

		private bool HasRefOrOutParams (ImmutableArray<IParameterSymbol> parameters)
			=> parameters.Any (p => p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref);

		private static string FormatRefKind (IParameterSymbol p)
		{
			switch (p.RefKind) {
				case RefKind.Out:
					return "out";
				case RefKind.Ref:
					return "ref";
				default:
					return "";
			}
		}
	}
}
