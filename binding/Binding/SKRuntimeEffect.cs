using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp
{
	public unsafe class SKRuntimeEffect : SKObject, ISKReferenceCounted
	{
		private string[] children;
		private string[] inputs;

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

		public int InputSize =>
			(int)SkiaApi.sk_runtimeeffect_get_input_size (Handle);

		public IReadOnlyList<string> Children =>
			children ??= GetChildrenNames ().ToArray ();

		public IReadOnlyList<string> Inputs =>
			inputs ??= GetInputNames ().ToArray ();

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

		private IEnumerable<string> GetInputNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_inputs_count (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_input_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
		}

		// ToShader

		public SKShader ToShader (bool isOpaque) =>
			ToShader (null, null, null, isOpaque);

		private SKShader ToShader (SKData inputs, bool isOpaque) =>
			ToShader (inputs, null, null, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, bool isOpaque) =>
			ToShader (inputs.ToData (), null, null, isOpaque);

		private SKShader ToShader (SKData inputs, SKShader[] children, bool isOpaque) =>
			ToShader (inputs, children, null, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, SKRuntimeEffectChildren children, bool isOpaque) =>
			ToShader (inputs.ToData (), children.ToArray (), null, isOpaque);

		private SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs, children, &localMatrix, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, SKRuntimeEffectChildren children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs.ToData (), children.ToArray (), &localMatrix, isOpaque);

		private SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix* localMatrix, bool isOpaque)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, inputsHandle, ch, (IntPtr)childrenHandles.Length, localMatrix, isOpaque));
			}
		}

		// ToColorFilter

		public SKColorFilter ToColorFilter () =>
			ToColorFilter ((SKData)null, null);

		public SKColorFilter ToColorFilter (SKRuntimeEffectInputs inputs) =>
			ToColorFilter (inputs.ToData (), null);

		private SKColorFilter ToColorFilter (SKData inputs) =>
			ToColorFilter (inputs, null);

		public SKColorFilter ToColorFilter (SKRuntimeEffectInputs inputs, SKRuntimeEffectChildren children) =>
			ToColorFilter (inputs.ToData (), children.ToArray ());

		private SKColorFilter ToColorFilter (SKData inputs, SKShader[] children)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				return SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, inputsHandle, ch, (IntPtr)childrenHandles.Length));
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}

	public unsafe class SKRuntimeEffectInputs : IEnumerable<string>
	{
		internal struct Variable
		{
			public int Index { get; set; }

			public string Name { get; set; }

			public int Offset { get; set; }

			public int Size { get; set; }
		}

		private readonly string[] names;
		private readonly Dictionary<string, Variable> variables;
		private SKData data;

		public SKRuntimeEffectInputs (SKRuntimeEffect effect)
		{
			if (effect == null)
				throw new ArgumentNullException (nameof (effect));

			names = effect.Inputs.ToArray ();
			variables = new Dictionary<string, Variable> ();
			data = SKData.Create (effect.InputSize);

			for (var i = 0; i < names.Length; i++) {
				var name = names[i];
				var input = SkiaApi.sk_runtimeeffect_get_input_from_index (effect.Handle, i);
				variables[name] = new Variable {
					Index = i,
					Name = name,
					Offset = (int)SkiaApi.sk_runtimeeffect_variable_get_offset (input),
					Size = (int)SkiaApi.sk_runtimeeffect_variable_get_size_in_bytes (input),
				};
			}
		}

		public IReadOnlyList<string> Names =>
			names;

		internal IReadOnlyList<Variable> Variables =>
			variables.Values.OrderBy (v => v.Index).ToArray ();

		public int Count =>
			names.Length;

		public void Reset () =>
			data = SKData.Create (data.Size);

		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		public SKRuntimeEffectInput this[string name] {
			set => Add (name, value);
		}

		public void Add (string name, SKRuntimeEffectInput value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			var variable = variables[name];
			var slice = data.Span.Slice (variable.Offset, variable.Size);

			if (value.IsEmpty) {
				slice.Fill (0);
				return;
			}

			if (value.Size != variable.Size)
				throw new ArgumentException ($"Value size of {value.Size} does not match variable size of {variable.Size}.", nameof (value));

			// TODO: either check or convert data types - for example int and float are both 4 bytes, but not the same byte[] value

			value.WriteTo (slice);
		}

		public SKData ToData () =>
			SKData.CreateCopy (data.Data, data.Size);

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

	public unsafe readonly ref struct SKRuntimeEffectInput
	{
		private enum DataType
		{
			Empty,

			Byte,
			ByteArray,
			Int,
			IntArray,
			Float,
			FloatArray
		}

		public static SKRuntimeEffectInput Empty => default;

		// fields

		private readonly DataType type;
		private readonly int size;

		private readonly byte byteValue;
		private readonly int intValue;
		private readonly float floatValue;

		private readonly ReadOnlySpan<byte> byteArray;
		private readonly ReadOnlySpan<int> intArray;
		private readonly ReadOnlySpan<float> floatArray;

		// ctor

		private SKRuntimeEffectInput (
			DataType type, int size,
			byte byteValue = default, int intValue = default, float floatValue = default,
			ReadOnlySpan<byte> byteArray = default, ReadOnlySpan<int> intArray = default, ReadOnlySpan<float> floatArray = default)
		{
			this.type = type;
			this.size = size;

			this.byteValue = byteValue;
			this.intValue = intValue;
			this.floatValue = floatValue;

			this.byteArray = byteArray;
			this.intArray = intArray;
			this.floatArray = floatArray;
		}

		// properties

		public bool IsEmpty => type == DataType.Empty;

		public int Size => size;

		// converters

		public static implicit operator SKRuntimeEffectInput (bool value) =>
			new SKRuntimeEffectInput (DataType.Byte, sizeof (byte), byteValue: value ? (byte)1 : (byte)0);

		public static implicit operator SKRuntimeEffectInput (int value) =>
			new SKRuntimeEffectInput (DataType.Int, sizeof (int), intValue: value);

		public static implicit operator SKRuntimeEffectInput (float value) =>
			new SKRuntimeEffectInput (DataType.Float, sizeof (float), floatValue: value);

		public static implicit operator SKRuntimeEffectInput (int[] value) => (ReadOnlySpan<int>)value;

		public static implicit operator SKRuntimeEffectInput (Span<int> value) => (ReadOnlySpan<int>)value;

		public static implicit operator SKRuntimeEffectInput (ReadOnlySpan<int> value) =>
			new SKRuntimeEffectInput (DataType.IntArray, sizeof (int) * value.Length, intArray: value);

		public static implicit operator SKRuntimeEffectInput (float[] value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectInput (Span<float> value) => (ReadOnlySpan<float>)value;

		public static implicit operator SKRuntimeEffectInput (ReadOnlySpan<float> value) =>
			new SKRuntimeEffectInput (DataType.FloatArray, sizeof (float) * value.Length, floatArray: value);

		public static implicit operator SKRuntimeEffectInput (float[][] value)
		{
			var floats = new List<float> ();
			foreach (var array in value) {
				floats.AddRange (array);
			}
			return floats.ToArray ();
		}

		// writer

		public void WriteTo (Span<byte> data)
		{
			switch (type) {
				case DataType.Byte:
					fixed (void* v = &byteValue)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
				case DataType.ByteArray:
					fixed (void* v = byteArray)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
				case DataType.Int:
					fixed (void* v = &intValue)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
				case DataType.IntArray:
					fixed (void* v = intArray)
						new ReadOnlySpan<byte> (v, size).CopyTo (data);
					break;
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
