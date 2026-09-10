#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp
{
	/// <summary>Represents a compiled SkSL runtime effect that can be used to create shaders, color filters, or blenders.</summary>
	/// <remarks />
	public unsafe class SKRuntimeEffect : SKObject, ISKReferenceCounted
	{
		private string[] children;
		private string[] uniforms;

		internal SKRuntimeEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Create*

		/// <summary>Compiles SkSL source code into a shader runtime effect.</summary>
		/// <param name="sksl">The SkSL shader source code.</param>
		/// <param name="errors">Returns any compilation errors.</param>
		/// <returns>A new runtime effect, or <see langword="null" /> if compilation failed.</returns>
		/// <remarks />
		public static SKRuntimeEffect CreateShader (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make_for_shader (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		/// <summary>Compiles SkSL source code into a color filter runtime effect.</summary>
		/// <param name="sksl">The SkSL color filter source code.</param>
		/// <param name="errors">Returns any compilation errors.</param>
		/// <returns>A new runtime effect, or <see langword="null" /> if compilation failed.</returns>
		/// <remarks />
		public static SKRuntimeEffect CreateColorFilter (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make_for_color_filter (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		/// <summary>Compiles SkSL source code into a blender runtime effect.</summary>
		/// <param name="sksl">The SkSL blender source code.</param>
		/// <param name="errors">Returns any compilation errors.</param>
		/// <returns>A new runtime effect, or <see langword="null" /> if compilation failed.</returns>
		/// <remarks />
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

		/// <summary>Compiles SkSL source code and returns a shader builder.</summary>
		/// <param name="sksl">The SkSL shader source code.</param>
		/// <returns>A new shader builder.</returns>
		/// <remarks />
		public static SKRuntimeShaderBuilder BuildShader (string sksl)
		{
			var effect = CreateShader (sksl, out var errors);
			ValidateResult (effect, errors);
			return new SKRuntimeShaderBuilder (effect);
		}

		/// <summary>Compiles SkSL source code and returns a color filter builder.</summary>
		/// <param name="sksl">The SkSL color filter source code.</param>
		/// <returns>A new color filter builder.</returns>
		/// <remarks />
		public static SKRuntimeColorFilterBuilder BuildColorFilter (string sksl)
		{
			var effect = CreateColorFilter (sksl, out var errors);
			ValidateResult (effect, errors);
			return new SKRuntimeColorFilterBuilder (effect);
		}

		/// <summary>Compiles SkSL source code and returns a blender builder.</summary>
		/// <param name="sksl">The SkSL blender source code.</param>
		/// <returns>A new blender builder.</returns>
		/// <remarks />
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

		/// <summary>Gets the total size in bytes required for all uniforms.</summary>
		/// <value>The uniform data size in bytes.</value>
		/// <remarks />
		public int UniformSize {
			get {
				var size = (int)SkiaApi.sk_runtimeeffect_get_uniform_byte_size (Handle);
				GC.KeepAlive (this);
				return size;
			}
		}

		/// <summary>Gets the names of child effects declared in the SkSL source.</summary>
		/// <value>A list of child effect names.</value>
		/// <remarks />
		public IReadOnlyList<string> Children =>
			children ??= GetChildrenNames ().ToArray ();

		/// <summary>Gets the names of uniforms declared in the SkSL source.</summary>
		/// <value>A list of uniform names.</value>
		/// <remarks />
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
			GC.KeepAlive (this);
		}

		private IEnumerable<string> GetUniformNames ()
		{
			var count = (int)SkiaApi.sk_runtimeeffect_get_uniforms_size (Handle);
			using var str = new SKString ();
			for (var i = 0; i < count; i++) {
				SkiaApi.sk_runtimeeffect_get_uniform_name (Handle, i, str.Handle);
				yield return str.ToString ();
			}
			GC.KeepAlive (this);
		}

		// ToShader

		/// <summary>Creates a shader from this runtime effect with default uniforms.</summary>
		/// <returns>A new shader.</returns>
		/// <remarks />
		public SKShader ToShader () =>
			ToShader (null, null, null);

		/// <summary>Creates a shader from this runtime effect with the specified uniforms.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <returns>A new shader.</returns>
		/// <remarks />
		public SKShader ToShader (SKRuntimeEffectUniforms uniforms) =>
			ToShader (uniforms.ToData (), null, null);

		/// <summary>Creates a shader from this runtime effect with the specified uniforms and children.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <param name="children">The child effects to use.</param>
		/// <returns>A new shader.</returns>
		/// <remarks />
		public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToShader (uniforms.ToData (), children.ToArray (), null);

		/// <summary>Creates a shader from this runtime effect with the specified uniforms, children, and local matrix.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <param name="children">The child effects to use.</param>
		/// <param name="localMatrix">The local transformation matrix to apply.</param>
		/// <returns>A new shader.</returns>
		/// <remarks />
		public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix) =>
			ToShader (uniforms.ToData (), children.ToArray (), &localMatrix);

		private SKShader ToShader (SKData uniforms, SKObject[] children, SKMatrix* localMatrix)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				var shader = SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length, localMatrix));
				GC.KeepAlive (uniforms);
				GC.KeepAlive (this);
				return shader;
			}
		}

		// ToColorFilter

		/// <summary>Creates a color filter from this runtime effect with default uniforms.</summary>
		/// <returns>A new color filter.</returns>
		/// <remarks />
		public SKColorFilter ToColorFilter () =>
			ToColorFilter ((SKData)null, null);

		/// <summary>Creates a color filter from this runtime effect with the specified uniforms.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <returns>A new color filter.</returns>
		/// <remarks />
		public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms) =>
			ToColorFilter (uniforms.ToData (), null);

		private SKColorFilter ToColorFilter (SKData uniforms) =>
			ToColorFilter (uniforms, null);

		/// <summary>Creates a color filter from this runtime effect with the specified uniforms and children.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <param name="children">The child effects to use.</param>
		/// <returns>A new color filter.</returns>
		/// <remarks />
		public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToColorFilter (uniforms.ToData (), children.ToArray ());

		private SKColorFilter ToColorFilter (SKData uniforms, SKObject[] children)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				var colorFilter = SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length));
				GC.KeepAlive (uniforms);
				GC.KeepAlive (this);
				return colorFilter;
			}
		}

		// ToBlender

		/// <summary>Creates a blender from this runtime effect with default uniforms.</summary>
		/// <returns>A new blender.</returns>
		/// <remarks />
		public SKBlender ToBlender () =>
			ToBlender ((SKData)null, null);

		/// <summary>Creates a blender from this runtime effect with the specified uniforms.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <returns>A new blender.</returns>
		/// <remarks />
		public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms) =>
			ToBlender (uniforms.ToData (), null);

		private SKBlender ToBlender (SKData uniforms) =>
			ToBlender (uniforms, null);

		/// <summary>Creates a blender from this runtime effect with the specified uniforms and children.</summary>
		/// <param name="uniforms">The uniform values to use.</param>
		/// <param name="children">The child effects to use.</param>
		/// <returns>A new blender.</returns>
		/// <remarks />
		public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children) =>
			ToBlender (uniforms.ToData (), children.ToArray ());

		private SKBlender ToBlender (SKData uniforms, SKObject[] children)
		{
			var uniformsHandle = uniforms?.Handle ?? IntPtr.Zero;
			using var childrenHandles = Utils.RentHandlesArray (children, true);

			fixed (IntPtr* ch = childrenHandles) {
				var blender = SKBlender.GetObject (SkiaApi.sk_runtimeeffect_make_blender (Handle, uniformsHandle, ch, (IntPtr)childrenHandles.Length));
				GC.KeepAlive (uniforms);
				GC.KeepAlive (this);
				return blender;
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}

	/// <summary>Represents a collection of uniform values for an <see cref="T:SkiaSharp.SKRuntimeEffect" />.</summary>
	/// <remarks>This class manages uniform data that is passed to SkSL shaders, color filters, and blenders. Uniforms are identified by name and can hold various data types including floats, integers, colors, and matrices.</remarks>
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectUniforms" /> class for the specified runtime effect.</summary>
		/// <param name="effect">The runtime effect that defines the available uniforms.</param>
		/// <remarks>The constructor initializes the uniform data buffer based on the uniforms declared in the effect's SkSL code.</remarks>
		public SKRuntimeEffectUniforms (SKRuntimeEffect effect)
		{
			if (effect == null)
				throw new ArgumentNullException (nameof (effect));

			names = effect.Uniforms.ToArray ();
			uniforms = new Dictionary<string, Variable> (names.Length);
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

		/// <summary>Gets the list of uniform names defined by the runtime effect.</summary>
		/// <value>A read-only list of uniform names.</value>
		/// <remarks />
		public IReadOnlyList<string> Names =>
			names;

		internal IReadOnlyList<Variable> Variables =>
			uniforms.Values.OrderBy (v => v.Index).ToArray ();

		/// <summary>Gets the number of uniforms in this collection.</summary>
		/// <value>The number of uniforms defined by the runtime effect.</value>
		/// <remarks />
		public int Count =>
			names.Length;

		/// <summary>Gets the total size of all uniform data in bytes.</summary>
		/// <value>The total size of the uniform data buffer in bytes.</value>
		/// <remarks />
		public int Size =>
			(int)data.Size;

		/// <summary>Resets all uniform values to their default state (zero).</summary>
		/// <remarks />
		public void Reset ()
		{
			if (data.Size == 0)
				return;

			var old = data;
			data = SKData.Create (old.Size);
			old.Dispose ();
		}

		/// <summary>Determines whether a uniform with the specified name exists in this collection.</summary>
		/// <param name="name">The name of the uniform to check for.</param>
		/// <returns><see langword="true" /> if a uniform with the specified name exists; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		/// <summary>Sets the value of the uniform with the specified name.</summary>
		/// <param name="name">The name of the uniform as declared in the SkSL code.</param>
		/// <value>The value to assign to the uniform.</value>
		/// <remarks>The value type must be compatible with the uniform type declared in the SkSL code.</remarks>
		public SKRuntimeEffectUniform this[string name] {
			set => Add (name, value);
		}

		/// <summary>Adds or updates a uniform value by name.</summary>
		/// <param name="name">The name of the uniform as declared in the SkSL code.</param>
		/// <param name="value">The value to assign to the uniform.</param>
		/// <remarks>The value type must be compatible with the uniform type declared in the SkSL code.</remarks>
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

		/// <summary>Creates a copy of the uniform data as an <see cref="T:SkiaSharp.SKData" /> object.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKData" /> object containing a copy of the uniform data.</returns>
		/// <remarks>The returned data can be passed to methods like <see cref="M:SkiaSharp.SKRuntimeEffect.ToShader(SkiaSharp.SKRuntimeEffectUniforms)" />.</remarks>
		public SKData ToData ()
		{
			if (data.Size == 0)
				return SKData.Empty;

			return SKData.CreateCopy (data.Data, data.Size);
		}

		/// <summary>Returns an enumerator that iterates through the uniform names.</summary>
		/// <returns>An enumerator that can be used to iterate through the uniform names.</returns>
		/// <remarks />
		IEnumerator IEnumerable.GetEnumerator () =>
			GetEnumerator ();

		/// <summary>Returns an enumerator that iterates through the uniform names.</summary>
		/// <returns>An enumerator that can be used to iterate through the uniform names.</returns>
		/// <remarks />
		public IEnumerator<string> GetEnumerator () =>
			((IEnumerable<string>)names).GetEnumerator ();

		/// <summary>Releases all resources used by this object.</summary>
		/// <remarks />
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

	/// <summary>Represents a collection of child shaders or color filters for use with <see cref="T:SkiaSharp.SKRuntimeEffect" />.</summary>
	/// <remarks />
	public class SKRuntimeEffectChildren : IEnumerable<string>, IDisposable
	{
		private readonly string[] names;
		private readonly SKObject[] children;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKRuntimeEffectChildren" /> for the specified effect.</summary>
		/// <param name="effect">The runtime effect that defines the available child slots.</param>
		/// <remarks />
		public SKRuntimeEffectChildren (SKRuntimeEffect effect)
		{
			_ = effect ?? throw new ArgumentNullException (nameof (effect));

			names = effect.Children.ToArray ();
			children = new SKObject[names.Length];
		}

		/// <summary>Gets the list of child slot names defined by the runtime effect.</summary>
		/// <value>A read-only list of child slot names.</value>
		/// <remarks />
		public IReadOnlyList<string> Names =>
			names;

		/// <summary>Gets the number of child slots defined by the runtime effect.</summary>
		/// <value>The number of child slots.</value>
		/// <remarks />
		public int Count =>
			names.Length;

		/// <summary>Clears all assigned child objects from all slots.</summary>
		/// <remarks />
		public void Reset () =>
			Array.Clear (children, 0, children.Length);

		/// <summary>Determines whether a child slot with the specified name exists.</summary>
		/// <param name="name">The name of the child slot to check.</param>
		/// <returns><see langword="true" /> if a child slot with the name exists; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (string name) =>
			Array.IndexOf (names, name) != -1;

		/// <summary>Sets the child object for the specified slot.</summary>
		/// <param name="name">The name of the child slot.</param>
		/// <value>The child object assigned to the slot.</value>
		/// <remarks />
		public SKRuntimeEffectChild? this[string name] {
			set => Add (name, value);
		}

		/// <summary>Adds a child object to the specified child slot.</summary>
		/// <param name="name">The name of the child slot as defined in the SkSL shader.</param>
		/// <param name="value">The shader, color filter, or blender to assign to the child slot.</param>
		/// <remarks />
		public void Add (string name, SKRuntimeEffectChild? value)
		{
			var index = Array.IndexOf (names, name);

			if (index == -1)
				throw new ArgumentOutOfRangeException (name, $"Variable was not found for name: '{name}'.");

			children[index] = value?.Value;
		}

		/// <summary>Returns all child objects as an array.</summary>
		/// <returns>An array containing all assigned child objects.</returns>
		/// <remarks />
		public SKObject[] ToArray () =>
			children.ToArray ();

		/// <summary>Returns an enumerator that iterates through the child slot names.</summary>
		/// <returns>An enumerator for the child slot names.</returns>
		/// <remarks />
		IEnumerator IEnumerable.GetEnumerator () =>
			GetEnumerator ();

		/// <summary>Returns an enumerator that iterates through the child slot names.</summary>
		/// <returns>An enumerator for the child slot names.</returns>
		/// <remarks />
		public IEnumerator<string> GetEnumerator () =>
			((IEnumerable<string>)names).GetEnumerator ();

		/// <summary>Releases all resources used by this instance.</summary>
		/// <remarks />
		public void Dispose ()
		{
		}
	}

	/// <summary>Represents a uniform value that can be passed to an <see cref="T:SkiaSharp.SKRuntimeEffect" />.</summary>
	/// <remarks>This ref struct wraps various data types (floats, integers, colors, matrices) and provides implicit conversions for convenient assignment to runtime effect uniforms.</remarks>
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

		/// <summary>Gets an empty uniform value.</summary>
		/// <value>An empty <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> instance.</value>
		/// <remarks />
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

		/// <summary>Gets a value indicating whether this uniform is empty.</summary>
		/// <value><see langword="true" /> if this uniform is empty; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsEmpty => Type == DataType.Empty;

		/// <summary>Gets the size of the uniform data in bytes.</summary>
		/// <value>The size of the uniform data in bytes.</value>
		/// <remarks />
		public int Size { get; }

		internal DataType Type { get; }

		// converters

		// float

		/// <summary>Converts a <see cref="T:System.Single" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The float value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the float value.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (float value) =>
			new SKRuntimeEffectUniform (DataType.Float, sizeof (float), floatValue: value);

		/// <summary>Converts an array of <see cref="T:System.Single" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The float array to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the float array.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (float[] value) => (ReadOnlySpan<float>)value;

		/// <summary>Converts a <see cref="T:System.Span`1" /> of <see cref="T:System.Single" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The span of floats to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the float values.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (Span<float> value) => (ReadOnlySpan<float>)value;

		/// <summary>Converts a <see cref="T:System.ReadOnlySpan`1" /> of <see cref="T:System.Single" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The read-only span of floats to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the float values.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (ReadOnlySpan<float> value) =>
			new SKRuntimeEffectUniform (DataType.FloatArray, sizeof (float) * value.Length, floatArray: value);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The point value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the point as a float2.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKPoint value) => (ReadOnlySpan<float>)new[] { value.X, value.Y };

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSize" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The size value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the size as a float2.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKSize value) => (ReadOnlySpan<float>)new[] { value.Width, value.Height };

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint3" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The 3D point value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the point as a float3.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKPoint3 value) => (ReadOnlySpan<float>)new[] { value.X, value.Y, value.Z };

		// int

		/// <summary>Converts an <see cref="T:System.Int32" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The integer value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the integer value.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (int value) =>
			new SKRuntimeEffectUniform (DataType.Int32, sizeof (int), intValue: value);

		/// <summary>Converts an array of <see cref="T:System.Int32" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The integer array to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the integer array.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (int[] value) => (ReadOnlySpan<int>)value;

		/// <summary>Converts a <see cref="T:System.Span`1" /> of <see cref="T:System.Int32" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The span of integers to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the integer values.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (Span<int> value) => (ReadOnlySpan<int>)value;

		/// <summary>Converts a <see cref="T:System.ReadOnlySpan`1" /> of <see cref="T:System.Int32" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The read-only span of integers to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the integer values.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (ReadOnlySpan<int> value) =>
			new SKRuntimeEffectUniform (DataType.Int32Array, sizeof (int) * value.Length, intArray: value);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPointI" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The integer point value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the point as an int2.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKPointI value) => (ReadOnlySpan<int>)new[] { value.X, value.Y };

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSizeI" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The integer size value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the size as an int2.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKSizeI value) => (ReadOnlySpan<int>)new[] { value.Width, value.Height };

		// color

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColor" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The color value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the color value.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKColor value) => (SKColorF)value;

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColorF" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The color value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the color value.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKColorF value) =>
			new SKRuntimeEffectUniform (DataType.Color, sizeof (float) * 4, colorValue: value);

		// float matrix

		/// <summary>Converts a jagged array of <see cref="T:System.Single" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The jagged float array to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the flattened float values.</returns>
		/// <remarks>The jagged array is flattened into a single contiguous array of floats.</remarks>
		public static implicit operator SKRuntimeEffectUniform (float[][] value)
		{
			var floats = new List<float> ();
			foreach (var array in value) {
				floats.AddRange (array);
			}
			return floats.ToArray ();
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKMatrix" /> to an <see cref="T:SkiaSharp.SKRuntimeEffectUniform" />.</summary>
		/// <param name="value">The matrix value to convert.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRuntimeEffectUniform" /> containing the matrix values as a float array.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectUniform (SKMatrix value) => value.Values;

		// writer

		/// <summary>Writes the uniform data to the specified byte span.</summary>
		/// <param name="data">The destination span to write the uniform data to.</param>
		/// <remarks>The destination span must have sufficient capacity to hold the uniform data.</remarks>
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

	/// <summary>Represents a child effect that can be passed to a runtime effect, wrapping a shader, color filter, or blender.</summary>
	/// <remarks />
	public unsafe readonly struct SKRuntimeEffectChild
	{
		private readonly SKObject value;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectChild" /> struct that wraps a shader.</summary>
		/// <param name="shader">The shader to wrap as a child effect.</param>
		/// <remarks />
		public SKRuntimeEffectChild (SKShader shader)
		{
			value = shader;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectChild" /> struct that wraps a color filter.</summary>
		/// <param name="colorFilter">The color filter to wrap as a child effect.</param>
		/// <remarks />
		public SKRuntimeEffectChild (SKColorFilter colorFilter)
		{
			value = colorFilter;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectChild" /> struct that wraps a blender.</summary>
		/// <param name="blender">The blender to wrap as a child effect.</param>
		/// <remarks />
		public SKRuntimeEffectChild (SKBlender blender)
		{
			value = blender;
		}

		/// <summary>Gets the underlying object wrapped by this child effect.</summary>
		/// <value>The wrapped shader, color filter, or blender.</value>
		/// <remarks />
		public SKObject Value => value;

		/// <summary>Gets the shader wrapped by this child effect, or <see langword="null" /> if this child wraps a different type.</summary>
		/// <value>The wrapped shader, or <see langword="null" />.</value>
		/// <remarks />
		public SKShader Shader => value as SKShader;

		/// <summary>Gets the color filter wrapped by this child effect, or <see langword="null" /> if this child wraps a different type.</summary>
		/// <value>The wrapped color filter, or <see langword="null" />.</value>
		/// <remarks />
		public SKColorFilter ColorFilter => value as SKColorFilter;

		/// <summary>Gets the blender wrapped by this child effect, or <see langword="null" /> if this child wraps a different type.</summary>
		/// <value>The wrapped blender, or <see langword="null" />.</value>
		/// <remarks />
		public SKBlender Blender => value as SKBlender;

		/// <summary>Implicitly converts a shader to a child effect.</summary>
		/// <param name="shader">The shader to convert.</param>
		/// <returns>A new child effect wrapping the shader.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectChild (SKShader shader) => new (shader);

		/// <summary>Implicitly converts a color filter to a child effect.</summary>
		/// <param name="colorFilter">The color filter to convert.</param>
		/// <returns>A new child effect wrapping the color filter.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectChild (SKColorFilter colorFilter) => new (colorFilter);

		/// <summary>Implicitly converts a blender to a child effect.</summary>
		/// <param name="blender">The blender to convert.</param>
		/// <returns>A new child effect wrapping the blender.</returns>
		/// <remarks />
		public static implicit operator SKRuntimeEffectChild (SKBlender blender) => new (blender);
	}

	/// <summary>The exception that is thrown when a runtime effect builder encounters an error.</summary>
	/// <remarks />
	public class SKRuntimeEffectBuilderException : ApplicationException
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectBuilderException" /> class with the specified error message.</summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <remarks />
		public SKRuntimeEffectBuilderException (string message)
			: base (message)
		{
		}
	}

	/// <summary>Base class for building runtime effects with uniforms and child effects.</summary>
	/// <remarks />
	public class SKRuntimeEffectBuilder : IDisposable
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeEffectBuilder" /> class for the specified runtime effect.</summary>
		/// <param name="effect">The compiled runtime effect to build upon.</param>
		/// <remarks />
		public SKRuntimeEffectBuilder (SKRuntimeEffect effect)
		{
			Effect = effect;

			Uniforms = new SKRuntimeEffectUniforms (effect);
			Children = new SKRuntimeEffectChildren (effect);
		}

		/// <summary>Gets the runtime effect this builder is configured for.</summary>
		/// <value>The runtime effect instance.</value>
		/// <remarks />
		public SKRuntimeEffect Effect { get; }

		/// <summary>Gets the collection of uniform values for this builder.</summary>
		/// <value>The uniforms collection that can be used to set uniform values by name.</value>
		/// <remarks />
		public SKRuntimeEffectUniforms Uniforms { get; }

		/// <summary>Gets the collection of child effects for this builder.</summary>
		/// <value>The child effects collection that can be used to set shader, color filter, or blender children.</value>
		/// <remarks />
		public SKRuntimeEffectChildren Children { get; }

		/// <summary>Releases all resources used by this builder.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Uniforms.Dispose ();
			Children.Dispose ();
			Effect.Dispose ();
		}
	}

	/// <summary>A builder for creating shaders from SkSL runtime effects.</summary>
	/// <remarks />
	public class SKRuntimeShaderBuilder : SKRuntimeEffectBuilder
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeShaderBuilder" /> class for the specified runtime effect.</summary>
		/// <param name="effect">The compiled shader runtime effect.</param>
		/// <remarks />
		public SKRuntimeShaderBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		/// <summary>Builds a shader using the configured uniforms and children.</summary>
		/// <returns>A new shader, or <see langword="null" /> if the build failed.</returns>
		/// <remarks />
		public SKShader Build () =>
			Effect.ToShader (Uniforms, Children);

		/// <summary>Builds a shader using the configured uniforms and children with a local transformation matrix.</summary>
		/// <param name="localMatrix">The local transformation matrix to apply to the shader.</param>
		/// <returns>A new shader, or <see langword="null" /> if the build failed.</returns>
		/// <remarks />
		public SKShader Build (SKMatrix localMatrix) =>
			Effect.ToShader (Uniforms, Children, localMatrix);
	}

	/// <summary>A builder for creating color filters from SkSL runtime effects.</summary>
	/// <remarks />
	public class SKRuntimeColorFilterBuilder : SKRuntimeEffectBuilder
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeColorFilterBuilder" /> class for the specified runtime effect.</summary>
		/// <param name="effect">The compiled color filter runtime effect.</param>
		/// <remarks />
		public SKRuntimeColorFilterBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		/// <summary>Builds a color filter using the configured uniforms and children.</summary>
		/// <returns>A new color filter, or <see langword="null" /> if the build failed.</returns>
		/// <remarks />
		public SKColorFilter Build () =>
			Effect.ToColorFilter (Uniforms, Children);
	}

	/// <summary>A builder for creating blenders from SkSL runtime effects.</summary>
	/// <remarks />
	public class SKRuntimeBlenderBuilder : SKRuntimeEffectBuilder
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRuntimeBlenderBuilder" /> class for the specified runtime effect.</summary>
		/// <param name="effect">The compiled blender runtime effect.</param>
		/// <remarks />
		public SKRuntimeBlenderBuilder (SKRuntimeEffect effect)
			: base (effect)
		{
		}

		/// <summary>Builds a blender using the configured uniforms and children.</summary>
		/// <returns>A new blender, or <see langword="null" /> if the build failed.</returns>
		/// <remarks />
		public SKBlender Build () =>
			Effect.ToBlender (Uniforms, Children);
	}
}
