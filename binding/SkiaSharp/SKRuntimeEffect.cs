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

		// Create

		public static SKRuntimeEffect Create (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		// properties

		public int UniformSize =>
			(int)SkiaApi.sk_runtimeeffect_get_uniform_size (Handle);

		public IReadOnlyList<string> Children =>
			children ??= GetChildrenNames ().ToArray ();

		public IReadOnlyList<string> Uniforms =>
			uniforms ??= GetUniformNames ().ToArray ();

		// Get*Names

		private IEnumerable<string> GetChildrenNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_children_count (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_child_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
		}

		private IEnumerable<string> GetUniformNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_uniforms_count (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_uniform_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
		}

		// ToShader

		public SKShader ToShader (bool isOpaque) =>
			ToShader (isOpaque, null, null, null);

		public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms) =>
			ToShader (isOpaque, uniforms.ToData (), null, null);

		public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToShader (isOpaque, uniforms.ToData (), children.ToArray (), null);

		public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix) =>
			ToShader (isOpaque, uniforms.ToData (), children.ToArray (), &localMatrix);

		private SKShader ToShader (bool isOpaque, SKData uniforms, SKShader[] children, SKMatrix* localMatrix)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length, localMatrix, isOpaque));
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

		private SKColorFilter ToColorFilter (SKData uniforms, SKShader[] children)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length));
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}

	public unsafe class SKRuntimeEffectUniforms : IEnumerable<string>
	{
		internal struct Variable
		{
			public int Index { get; set; }

			public string Name { get; set; }

			public int Offset { get; set; }

			public int Size { get; set; }
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
				var uniform = SkiaApi.sk_runtimeeffect_get_uniform_from_index (effect.Handle, i);
				uniforms[name] = new Variable {
					Index = i,
					Name = name,
					Offset = (int)SkiaApi.sk_runtimeeffect_uniform_get_offset (uniform),
					Size = (int)SkiaApi.sk_runtimeeffect_uniform_get_size_in_bytes (uniform),
				};
			}
		}

		public IReadOnlyList<string> Names =>
			names;

		internal IReadOnlyList<Variable> Variables =>
			uniforms.Values.OrderBy (v => v.Index).ToArray ();

		public int Count =>
			names.Length;

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
			var slice = data.Span.Slice (uniform.Offset, uniform.Size);

			if (value.IsEmpty) {
				slice.Fill (0);
				return;
			}

			if (value.Size != uniform.Size)
				throw new ArgumentException ($"Value size of {value.Size} does not match uniform size of {uniform.Size}.", nameof (value));

			// TODO: either check or convert data types - for example int and float are both 4 bytes, but not the same byte[] value

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
	}

	public class SKRuntimeEffectChildren : IEnumerable<string>
	{
		private readonly string[] names;
		private readonly SKShader[] children;

		public SKRuntimeEffectChildren (SKRuntimeEffect effect)
		{
			if (effect == null)
				throw new ArgumentNullException (nameof (effect));

			names = effect.Children.ToArray ();
			children = new SKShader[names.Length];
		}

		public IReadOnlyList<string> Names =>
			names;

		public int Count =>
			names.Length;

		public void Reset () =>
			Array.Clear (children, 0, children.Length);

		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		public SKShader this[string name] {
			set => Add (name, value);
		}

		public void Add (string name, SKShader value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			children[index] = value;
		}

		public SKShader[] ToArray () =>
			children.ToArray ();

		IEnumerator IEnumerable.GetEnumerator () =>
			GetEnumerator ();

		public IEnumerator<string> GetEnumerator () =>
			((IEnumerable<string>)names).GetEnumerator ();
	}

	public unsafe readonly ref struct SKRuntimeEffectUniform
	{
		private enum DataType
		{
			Empty,

			Float,
			FloatArray
		}

		public static SKRuntimeEffectUniform Empty => default;

		// fields

		private readonly DataType type;
		private readonly int size;

		private readonly float floatValue;

		private readonly ReadOnlySpan<float> floatArray;

		// ctor

		private SKRuntimeEffectUniform (
			DataType type, int size,
			float floatValue = default,
			ReadOnlySpan<float> floatArray = default)
		{
			this.type = type;
			this.size = size;

			this.floatValue = floatValue;

			this.floatArray = floatArray;
		}

		// properties

		public bool IsEmpty => type == DataType.Empty;

		public int Size => size;

		// converters

		public static implicit operator SKRuntimeEffectUniform (float value) =>
			new SKRuntimeEffectUniform (DataType.Float, sizeof (float), floatValue: value);

		public static implicit operator SKRuntimeEffectUniform (float[] value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectUniform (Span<float> value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectUniform (ReadOnlySpan<float> value) =>
			new SKRuntimeEffectUniform (DataType.FloatArray, sizeof (float) * value.Length, floatArray: value);

		public static implicit operator SKRuntimeEffectUniform (float[][] value)
		{
			var floats = new List<float> ();
			foreach (var array in value) {
				floats.AddRange (array);
			}
			return floats.ToArray ();
		}

		public static implicit operator SKRuntimeEffectUniform (SKMatrix value) => value.Values;

		// writer

		public void WriteTo (Span<byte> data)
		{
			switch (type) {
				case DataType.Float:
					fixed (void* v = &floatValue)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
				case DataType.FloatArray:
					fixed (void* v = floatArray)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
				case DataType.Empty:
				default:
					data.Fill (0);
					break;
			}
		}
	}
}
