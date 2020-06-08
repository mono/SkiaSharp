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

		public SKShader ToShader (SKData inputs, bool isOpaque) =>
			ToShader (inputs, null, null, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, bool isOpaque) =>
			ToShader (inputs.ToData (), null, null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, bool isOpaque) =>
			ToShader (inputs, children, null, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, SKRuntimeEffectChildren children, bool isOpaque) =>
			ToShader (inputs.ToData (), children.ToArray (), null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs, children, &localMatrix, isOpaque);

		public SKShader ToShader (SKRuntimeEffectInputs inputs, SKRuntimeEffectChildren children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs.ToData (), children.ToArray (), &localMatrix, isOpaque);

		internal unsafe SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix* localMatrix, bool isOpaque)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;

			IntPtr[] childrenHandles = null;
			var childLength = IntPtr.Zero;
			if (children != null && children.Length > 0) {
				childrenHandles = new IntPtr[children.Length];
				for (int i = 0; i < children.Length; i++) {
					childrenHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
				}
				childLength = (IntPtr)children.Length;
			}

			fixed (IntPtr* ch = childrenHandles) {
				return SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, inputsHandle, ch, childLength, localMatrix, isOpaque));
			}
		}

		// ToColorFilter

		public SKColorFilter ToColorFilter () =>
			ToColorFilter (null, null);

		public SKColorFilter ToColorFilter (SKData inputs) =>
			ToColorFilter (inputs, null);

		internal SKColorFilter ToColorFilter (SKData inputs, SKShader[] children)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;

			IntPtr[] childrenHandles = null;
			if (children != null && children.Length > 0) {
				childrenHandles = new IntPtr[children.Length];
				for (int i = 0; i < children.Length; i++) {
					childrenHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
				}
			}

			fixed (IntPtr* ch = childrenHandles) {
				return SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, inputsHandle, ch, (IntPtr)children?.Length));
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}

	public unsafe class SKRuntimeEffectInputs
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

		public void Set (string name, bool value)
		{
			var v = value ? (byte)1 : (byte)0;
			Set (name, &v, sizeof (byte));
		}

		public void Set (string name, int value) =>
			Set (name, &value, sizeof (int));

		public void Set (string name, int[] value)
		{
			fixed (void* v = value) {
				Set (name, v, sizeof (int) * (value?.Length ?? 0));
			}
		}

		public void Set (string name, float value) =>
			Set (name, &value, sizeof (float));

		public void Set (string name, float[] value)
		{
			fixed (void* v = value) {
				Set (name, v, sizeof (float) * (value?.Length ?? 0));
			}
		}

		public void Set (string name, float[][] value)
		{
			var floats = new List<float> ();
			foreach (var array in value) {
				floats.AddRange (array);
			}
			Set (name, floats.ToArray ());
		}

		private void Set (string name, void* value, int size)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			var variable = variables[name];
			var slice = data.Span.Slice (variable.Offset, variable.Size);

			if (value == null) {
				slice.Fill (0);
				return;
			}

			if (size != variable.Size)
				throw new ArgumentException ($"Value size of {size} does not match variable size of {variable.Size}.", nameof (value));

			var val = new Span<byte> (value, size);
			val.CopyTo (slice);
		}

		public SKData ToData () =>
			SKData.CreateCopy (data.Data, data.Size);
	}

	public class SKRuntimeEffectChildren
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

		public void Set (string name, SKShader value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			children[index] = value;
		}

		public SKShader[] ToArray () =>
			children.ToArray ();
	}
}
