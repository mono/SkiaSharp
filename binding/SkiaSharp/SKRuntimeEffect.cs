#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp
{
	public unsafe class SKRuntimeEffect : SKObject, ISKReferenceCounted
	{
		private string[] children;
		private string[] uniforms;

		internal SKRuntimeEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Create*

		public static SKRuntimeEffect CreateShader (ReadOnlySpan<char> sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make_for_shader (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		public static SKRuntimeEffect CreateColorFilter (ReadOnlySpan<char> sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make_for_color_filter (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		public static SKRuntimeEffect CreateBlender (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make_for_blender (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		// Build*

		public static SKRuntimeShaderBuilder BuildShader (ReadOnlySpan<char> sksl)
		{
			var effect = CreateShader (sksl, out var errors);
			ValidateResult (effect, errors);
			return new SKRuntimeShaderBuilder (effect);
		}

		public static SKRuntimeColorFilterBuilder BuildColorFilter (ReadOnlySpan<char> sksl)
		{
			var effect = CreateColorFilter (sksl, out var errors);
			ValidateResult (effect, errors);
			return new SKRuntimeColorFilterBuilder (effect);
		}

		public static SKRuntimeBlenderBuilder BuildBlender (string sksl)
		{
			var effect = CreateBlender (sksl, out var errors);
			ValidateResult (effect, errors);
			return new SKRuntimeBlenderBuilder (effect);
		}

		private static void ValidateResult (SKRuntimeEffect effect, string errors)
		{
			if (effect is null) {
				if (string.IsNullOrEmpty (errors))
					throw new SKRuntimeEffectBuilderException ($"Failed to compile the runtime effect. There was an unknown error.");
				else
					throw new SKRuntimeEffectBuilderException ($"Failed to compile the runtime effect. There was an error: {errors}");
			}
		}

		// properties

		public int UniformSize =>
			(int)SkiaApi.sk_runtimeeffect_get_uniform_byte_size (Handle);

		public IReadOnlyList<string> Children =>
			children ??= GetChildrenNames ().ToArray ();

		public IReadOnlyList<string> Uniforms =>
			uniforms ??= GetUniformNames ().ToArray ();

		// Get*Names

		private IEnumerable<string> GetChildrenNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_children_size (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_child_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
		}

		private IEnumerable<string> GetUniformNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_uniforms_size (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_uniform_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
		}

		// ToShader

		public SKShader ToShader () =>
			ToShader (null, null, null);

		public SKShader ToShader (SKRuntimeEffectUniforms uniforms) =>
			ToShader (uniforms.ToData (), null, null);

		public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToShader (uniforms.ToData (), children.ToArray (), null);

		public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix) =>
			ToShader (uniforms.ToData (), children.ToArray (), &localMatrix);

		private SKShader ToShader (SKData uniforms, SKObject[] children, SKMatrix* localMatrix)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length, localMatrix));
			}
		}

		// ToColorFilter

		public SKColorFilter ToColorFilter () =>
			ToColorFilter ((SKData)null, null);

		public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms) =>
			ToColorFilter (uniforms.ToData (), null);

		private SKColorFilter ToColorFilter (SKData uniforms) =>
			ToColorFilter (uniforms, null);

		public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToColorFilter (uniforms.ToData (), children.ToArray ());

		private SKColorFilter ToColorFilter (SKData uniforms, SKObject[] children)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length));
			}
		}

		// ToBlender

		public SKBlender ToBlender () =>
			ToBlender ((SKData)null, null);

		public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms) =>
			ToBlender (uniforms.ToData (), null);

		private SKBlender ToBlender (SKData uniforms) =>
			ToBlender (uniforms, null);

		public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToBlender (uniforms.ToData (), children.ToArray ());

		private SKBlender ToBlender (SKData uniforms, SKObject[] children)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKBlender.GetObject (SkiaApi.sk_runtimeeffect_make_blender (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length));
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}

	public unsafe class SKRuntimeEffectUniforms : IEnumerable<string>, IDisposable
	{
		internal readonly struct Variable
		{
			public Variable (int index, string name, SKRuntimeEffectUniformNative uniform)
			{
				Index = index;
				Name = name;
				Offset = (int)uniform.fOffset;
				Type = uniform.fType;
				Count = uniform.fCount;
				Flags = uniform.fFlags;
			}

			public int Index { get; }

			public string Name { get; }

			public int Offset { get; }

			public SKRuntimeEffectUniformTypeNative Type { get; }

			public int Count { get; }

			public SKRuntimeEffectUniformFlagsNative Flags { get; }

			public int ElementSize => Type switch {
				SKRuntimeEffectUniformTypeNative.Float => sizeof (float),
				SKRuntimeEffectUniformTypeNative.Float2 => sizeof (float) * 2,
				SKRuntimeEffectUniformTypeNative.Float3 => sizeof (float) * 3,
				SKRuntimeEffectUniformTypeNative.Float4 => sizeof (float) * 4,
				SKRuntimeEffectUniformTypeNative.Float2x2 => sizeof (float) * 2 * 2,
				SKRuntimeEffectUniformTypeNative.Float3x3 => sizeof (float) * 3 * 3,
				SKRuntimeEffectUniformTypeNative.Float4x4 => sizeof (float) * 4 * 4,
				SKRuntimeEffectUniformTypeNative.Int => sizeof (int),
				SKRuntimeEffectUniformTypeNative.Int2 => sizeof (int) * 2,
				SKRuntimeEffectUniformTypeNative.Int3 => sizeof (int) * 3,
				SKRuntimeEffectUniformTypeNative.Int4 => sizeof (int) * 4,
				_ => throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown variable type: '{Type}'"),
			};

			public int Size => ElementSize * Count;
		}

		private readonly string[] names;
		private readonly Dictionary<string, Variable> uniforms;
		private SKData data;

		public SKRuntimeEffectUniforms (SKRuntimeEffect effect)
		{
			if (effect == null)
				throw new ArgumentNullException (nameof (effect));

			names = effect.Uniforms.ToArray ();
			uniforms = new Dictionary<string, Variable> ();
			data = effect.UniformSize is int size && size > 0
				? SKData.Create (effect.UniformSize)
				: SKData.Empty;

			for (var i = 0; i < names.Length; i++) {
				var name = names[i];
				SKRuntimeEffectUniformNative uniform;
				SkiaApi.sk_runtimeeffect_get_uniform_from_index (effect.Handle, i, &uniform);
				uniforms[name] = new Variable (i, name, uniform);
			}
		}

		public IReadOnlyList<string> Names =>
			names;

		internal IReadOnlyList<Variable> Variables =>
			uniforms.Values.OrderBy (v => v.Index).ToArray ();

		public int Count =>
			names.Length;

		public int Size =>
			(int)data.Size;

		public void Reset ()
		{
			if (data.Size == 0)
				return;

			data = SKData.Create (data.Size);
		}

		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		public SKRuntimeEffectUniform this[string name] {
			set => Add (name, value);
		}

		public void Add (string name, SKRuntimeEffectUniform value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			var uniform = uniforms[name];

			// validate the types first
			if (!ValidateTypes (value.Type, uniform.Type, uniform.Flags.HasFlag (SKRuntimeEffectUniformFlagsNative.Array), uniform.Count))
				throw new ArgumentOutOfRangeException (nameof (value), $"Unable to write a '{value.Type}' value to a '{uniform.Type}' uniform.");

			var slice = data.Span.Slice (uniform.Offset, uniform.Size);

			// validate the sizes and then write
			value.WriteTo (slice);
		}

		public SKData ToData ()
		{
			if (data.Size == 0)
				return SKData.Empty;

			return SKData.CreateCopy (data.Data, data.Size);
		}

		IEnumerator IEnumerable.GetEnumerator () =>
			GetEnumerator ();

		public IEnumerator<string> GetEnumerator () =>
			((IEnumerable<string>)names).GetEnumerator ();

		public void Dispose () =>
			data.Dispose ();

		private bool ValidateTypes (SKRuntimeEffectUniform.DataType valueType, SKRuntimeEffectUniformTypeNative uniformType, bool isArray, int arraySize) =>
			valueType switch {
				SKRuntimeEffectUniform.DataType.Float => uniformType switch {
					SKRuntimeEffectUniformTypeNative.Float when !isArray => true,
					_ => false,
				},
				SKRuntimeEffectUniform.DataType.FloatArray => uniformType switch {
					SKRuntimeEffectUniformTypeNative.Float when isArray => true,
					SKRuntimeEffectUniformTypeNative.Float2 => true,
					SKRuntimeEffectUniformTypeNative.Float3 => true,
					SKRuntimeEffectUniformTypeNative.Float4 => true,
					SKRuntimeEffectUniformTypeNative.Float2x2 => true,
					SKRuntimeEffectUniformTypeNative.Float3x3 => true,
					SKRuntimeEffectUniformTypeNative.Float4x4 => true,
					_ => false,
				},
				SKRuntimeEffectUniform.DataType.Int32 => uniformType switch {
					SKRuntimeEffectUniformTypeNative.Int when !isArray => true,
					_ => false,
				},
				SKRuntimeEffectUniform.DataType.Int32Array => uniformType switch {
					SKRuntimeEffectUniformTypeNative.Int when isArray => true,
					SKRuntimeEffectUniformTypeNative.Int2 => true,
					SKRuntimeEffectUniformTypeNative.Int3 => true,
					SKRuntimeEffectUniformTypeNative.Int4 => true,
					_ => false,
				},
				SKRuntimeEffectUniform.DataType.Color => uniformType switch {
					SKRuntimeEffectUniformTypeNative.Float3 => true,
					SKRuntimeEffectUniformTypeNative.Float4 => true,
					_ => false,
				},
				_ => false,
			};
	}

	public class SKRuntimeEffectChildren : IEnumerable<string>, IDisposable
	{
		private readonly string[] names;
		private readonly SKObject[] children;

		public SKRuntimeEffectChildren (SKRuntimeEffect effect)
		{
			_ = effect ?? throw new ArgumentNullException (nameof (effect));

			names = effect.Children.ToArray ();
			children = new SKObject[names.Length];
		}

		public IReadOnlyList<string> Names =>
			names;

		public int Count =>
			names.Length;

		public void Reset () =>
			Array.Clear (children, 0, children.Length);

		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		public SKRuntimeEffectChild? this[string name] {
			set => Add (name, value);
		}

		public void Add (string name, SKRuntimeEffectChild? value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			children[index] = value?.Value;
		}

		public SKObject[] ToArray () =>
			children.ToArray ();

		IEnumerator IEnumerable.GetEnumerator () =>
			GetEnumerator ();

		public IEnumerator<string> GetEnumerator () =>
			((IEnumerable<string>)names).GetEnumerator ();

		public void Dispose ()
		{
		}
	}

	public unsafe readonly ref struct SKRuntimeEffectUniform
	{
		internal enum DataType
		{
			Empty,

			Float,
			FloatArray,
			Int32,
			Int32Array,
			Color,
		}

		public static SKRuntimeEffectUniform Empty => default;

		// fields

		private readonly float floatValue;
		private readonly ReadOnlySpan<float> floatArray;

		private readonly int intValue;
		private readonly ReadOnlySpan<int> intArray;

		private readonly SKColorF colorValue;

		// ctor

		private SKRuntimeEffectUniform (
			DataType type,
			int size,
			float floatValue = default,
			ReadOnlySpan<float> floatArray = default,
			int intValue = default,
			ReadOnlySpan<int> intArray = default,
			SKColorF colorValue = default)
		{
			Type = type;
			Size = size;

			this.floatValue = floatValue;
			this.floatArray = floatArray;

			this.intValue = intValue;
			this.intArray = intArray;

			this.colorValue = colorValue;
		}

		// properties

		public bool IsEmpty => Type == DataType.Empty;

		public int Size { get; }

		internal DataType Type { get; }

		// converters

		// float

		public static implicit operator SKRuntimeEffectUniform (float value) =>
			new SKRuntimeEffectUniform (DataType.Float, sizeof (float), floatValue: value);

		public static implicit operator SKRuntimeEffectUniform (float[] value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectUniform (Span<float> value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectUniform (ReadOnlySpan<float> value) =>
			new SKRuntimeEffectUniform (DataType.FloatArray, sizeof (float) * value.Length, floatArray: value);

		public static implicit operator SKRuntimeEffectUniform (SKPoint value) => (ReadOnlySpan<float>)new[] { value.X, value.Y };

		public static implicit operator SKRuntimeEffectUniform (SKSize value) => (ReadOnlySpan<float>)new[] { value.Width, value.Height };

		public static implicit operator SKRuntimeEffectUniform (SKPoint3 value) => (ReadOnlySpan<float>)new[] { value.X, value.Y, value.Z };

		// int

		public static implicit operator SKRuntimeEffectUniform (int value) =>
			new SKRuntimeEffectUniform (DataType.Int32, sizeof (int), intValue: value);

		public static implicit operator SKRuntimeEffectUniform (int[] value) => (ReadOnlySpan<int>)value;

		public static implicit operator SKRuntimeEffectUniform (Span<int> value) => (ReadOnlySpan<int>)value;

		public static implicit operator SKRuntimeEffectUniform (ReadOnlySpan<int> value) =>
			new SKRuntimeEffectUniform (DataType.Int32Array, sizeof (int) * value.Length, intArray: value);

		public static implicit operator SKRuntimeEffectUniform (SKPointI value) => (ReadOnlySpan<int>)new[] { value.X, value.Y };

		public static implicit operator SKRuntimeEffectUniform (SKSizeI value) => (ReadOnlySpan<int>)new[] { value.Width, value.Height };

		// color

		public static implicit operator SKRuntimeEffectUniform (SKColor value) => (SKColorF)value;

		public static implicit operator SKRuntimeEffectUniform (SKColorF value) =>
			new SKRuntimeEffectUniform (DataType.Color, sizeof (float) * 4, colorValue: value);

		// float matrix

		public static implicit operator SKRuntimeEffectUniform (float[][] value)
		{
			var totalLength = 0;
			foreach (var f in value) {
				totalLength += f.Length;
			}

			var floats = new float[totalLength];

			var offset = 0;
			foreach (var f in value)
			{
				Array.Copy (f, 0, floats, offset, f.Length);
				offset += f.Length;
			}

			return floats;
		}

		public static implicit operator SKRuntimeEffectUniform (SKMatrix value) => value.Values;

		// writer

		public void WriteTo (Span<byte> data)
		{
			switch (Type) {
				// float
				case DataType.Float when data.Length == sizeof (float):
					fixed (void* v = &floatValue)
						new ReadOnlySpan<byte> (v, Size).CopyTo (data);
					break;
				case DataType.Float:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown float data type length: {data.Length}");

				// float array
				case DataType.FloatArray when data.Length == sizeof (int) * floatArray.Length:
					fixed (void* v = floatArray)
						new ReadOnlySpan<byte> (v, Size).CopyTo (data);
					break;
				case DataType.FloatArray:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown float array data type length: {data.Length}");

				// int
				case DataType.Int32 when data.Length == sizeof (int):
					fixed (void* v = &intValue)
						new ReadOnlySpan<byte> (v, Size).CopyTo (data);
					break;
				case DataType.Int32:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown int data type length: {data.Length}");

				// int array
				case DataType.Int32Array when data.Length == sizeof (int) * intArray.Length:
					fixed (void* v = intArray)
						new ReadOnlySpan<byte> (v, Size).CopyTo (data);
					break;
				case DataType.Int32Array:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown int array data type length: {data.Length}");

				// colors
				case DataType.Color when data.Length == sizeof (float) * 3:
					void* vc3 = stackalloc[] { colorValue.Red, colorValue.Green, colorValue.Blue };
					new ReadOnlySpan<byte> (vc3, data.Length).CopyTo (data);
					break;
				case DataType.Color when data.Length == sizeof (float) * 4:
					void* vc4 = stackalloc[] { colorValue.Red, colorValue.Green, colorValue.Blue, colorValue.Alpha };
					new ReadOnlySpan<byte> (vc4, data.Length).CopyTo (data);
					break;
				case DataType.Color:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown color data type length: {data.Length}");

				// empty
				case DataType.Empty:
					data.Fill (0);
					break;

				// error
				default:
					throw new ArgumentOutOfRangeException (nameof (Type), $"Unknown data type: '{Type}'");
			}
		}
	}

	public unsafe readonly struct SKRuntimeEffectChild
	{
		private readonly SKObject value;

		public SKRuntimeEffectChild (SKShader shader)
		{
			value = shader;
		}

		public SKRuntimeEffectChild (SKColorFilter colorFilter)
		{
			value = colorFilter;
		}

		public SKRuntimeEffectChild (SKBlender blender)
		{
			value = blender;
		}

		public SKObject Value => value;

		public SKShader Shader => value as SKShader;

		public SKColorFilter ColorFilter => value as SKColorFilter;

		public SKBlender Blender => value as SKBlender;

		public static implicit operator SKRuntimeEffectChild (SKShader shader) => new (shader);

		public static implicit operator SKRuntimeEffectChild (SKColorFilter colorFilter) => new (colorFilter);

		public static implicit operator SKRuntimeEffectChild (SKBlender blender) => new (blender);
	}

	public class SKRuntimeEffectBuilderException : ApplicationException
	{
		public SKRuntimeEffectBuilderException (string message)
			: base (message)
		{
		}
	}

	public class SKRuntimeEffectBuilder : IDisposable
	{
		public SKRuntimeEffectBuilder (SKRuntimeEffect effect)
		{
			Effect = effect;

			Uniforms = new SKRuntimeEffectUniforms (effect);
			Children = new SKRuntimeEffectChildren (effect);
		}

		public SKRuntimeEffect Effect { get; }

		public SKRuntimeEffectUniforms Uniforms { get; }

		public SKRuntimeEffectChildren Children { get; }

		public void Dispose ()
		{
			Uniforms.Dispose ();
			Children.Dispose ();
			Effect.Dispose ();
		}
	}

	public class SKRuntimeShaderBuilder : SKRuntimeEffectBuilder
	{
		public SKRuntimeShaderBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		public SKShader Build () =>
			Effect.ToShader (Uniforms, Children);

		public SKShader Build (SKMatrix localMatrix) =>
			Effect.ToShader (Uniforms, Children, localMatrix);
	}

	public class SKRuntimeColorFilterBuilder : SKRuntimeEffectBuilder
	{
		public SKRuntimeColorFilterBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		public SKColorFilter Build () =>
			Effect.ToColorFilter (Uniforms, Children);
	}

	public class SKRuntimeBlenderBuilder : SKRuntimeEffectBuilder
	{
		public SKRuntimeBlenderBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		public SKBlender Build () =>
			Effect.ToBlender (Uniforms, Children);
	}
}
