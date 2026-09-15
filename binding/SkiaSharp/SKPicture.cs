#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>Recorded drawing operations made to a <see cref="T:SkiaSharp.SKCanvas" /> to be played back at a later time.</summary>
	/// <remarks>This base class handles serialization and a few other miscellany.</remarks>
	public unsafe class SKPicture : SKObject, ISKReferenceCounted
	{
		internal SKPicture (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPicture" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPicture" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Gets the non-zero value unique among all pictures.</summary>
		/// <value>The unique identifier for this picture.</value>
		/// <remarks />
		public uint UniqueId {
			get {
				var result = SkiaApi.sk_picture_get_unique_id (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the culling rectangle for this picture.</summary>
		/// <value>The culling rectangle used to optimize drawing.</value>
		/// <remarks>Operations recorded into this picture that attempt to draw outside the culling rectangle might not be drawn.</remarks>
		public SKRect CullRect {
			get {
				SKRect rect;
				SkiaApi.sk_picture_get_cull_rect (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		/// <summary>Gets the approximate number of bytes used by this picture.</summary>
		/// <value>The approximate byte count.</value>
		/// <remarks />
		public int ApproximateBytesUsed {
			get {
				var result = (int)SkiaApi.sk_picture_approximate_bytes_used (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the approximate number of drawing operations recorded in this picture.</summary>
		/// <value>The approximate operation count.</value>
		/// <remarks />
		public int ApproximateOperationCount =>
			GetApproximateOperationCount (false);

		/// <summary>Gets the approximate number of drawing operations recorded in this picture.</summary>
		/// <param name="includeNested">Whether to include operations from nested pictures.</param>
		/// <returns>The approximate operation count.</returns>
		/// <remarks />
		public int GetApproximateOperationCount(bool includeNested)
		{
			var result = SkiaApi.sk_picture_approximate_op_count (Handle, includeNested);
			GC.KeepAlive (this);
			return result;
		}

		// Serialize

		/// <summary>Serializes the picture into an SKData object.</summary>
		/// <returns>An SKData object containing the serialized picture.</returns>
		/// <remarks />
		public SKData Serialize ()
		{
			var result = SKData.GetObject (SkiaApi.sk_picture_serialize_to_data (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Serializes the picture to the specified .NET stream.</summary>
		/// <param name="stream">The .NET stream to write the serialized picture to.</param>
		/// <remarks />
		public void Serialize (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var managed = new SKManagedWStream (stream);
			Serialize (managed);
		}

		/// <summary>Serializes the picture to the specified stream.</summary>
		/// <param name="stream">The SKWStream to write the serialized picture to.</param>
		/// <remarks />
		public void Serialize (SKWStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			SkiaApi.sk_picture_serialize_to_stream (Handle, stream.Handle);
			GC.KeepAlive (stream);
			GC.KeepAlive (this);
		}

		// Playback

		/// <summary>Replays the recorded drawing commands onto the specified canvas.</summary>
		/// <param name="canvas">The canvas to play the drawing commands onto.</param>
		/// <remarks />
		public void Playback (SKCanvas canvas)
		{
			if (canvas is null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_picture_playback (Handle, canvas.Handle);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		// ToShader

		/// <summary>Creates a shader from this picture using the cull rect as the tile boundary.</summary>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKFilterMode.Nearest, null, null);

		/// <summary>Creates a shader from this picture with specified tile modes.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			ToShader (tmx, tmy, SKFilterMode.Nearest, null, null);

		/// <summary>Creates a shader from this picture with specified tile and filter modes.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <param name="filterMode">The filter mode to apply when scaling.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode) =>
			ToShader (tmx, tmy, filterMode, null, null);

		/// <summary>Creates a shader from this picture with specified tile modes and boundary.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <param name="tile">The rectangle defining the tile boundary.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile) =>
			ToShader (tmx, tmy, SKFilterMode.Nearest, null, &tile);

		/// <summary>Creates a shader from this picture with specified tile modes, filter mode, and boundary.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <param name="filterMode">The filter mode to apply when scaling.</param>
		/// <param name="tile">The rectangle defining the tile boundary.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile) =>
			ToShader (tmx, tmy, filterMode, null, &tile);

		/// <summary>Creates a shader from this picture with specified tile modes, transformation, and boundary.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <param name="localMatrix">The local transformation matrix to apply.</param>
		/// <param name="tile">The rectangle defining the tile boundary.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile) =>
			ToShader (tmx, tmy, SKFilterMode.Nearest, &localMatrix, &tile);

		/// <summary>Creates a shader from this picture with specified tile modes, filter mode, transformation, and boundary.</summary>
		/// <param name="tmx">The tile mode for the X axis.</param>
		/// <param name="tmy">The tile mode for the Y axis.</param>
		/// <param name="filterMode">The filter mode to apply when scaling.</param>
		/// <param name="localMatrix">The local transformation matrix to apply.</param>
		/// <param name="tile">The rectangle defining the tile boundary.</param>
		/// <returns>A new shader that renders this picture.</returns>
		/// <remarks />
		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile) =>
			ToShader (tmx, tmy, filterMode, &localMatrix, &tile);

		private SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix* localMatrix, SKRect* tile)
		{
			var result = SKShader.GetObject (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, filterMode, localMatrix, tile));
			GC.KeepAlive (this);
			return result;
		}

		// Deserialize

		/// <summary>Deserializes a picture from a native memory pointer.</summary>
		/// <param name="data">The pointer to the serialized picture data.</param>
		/// <param name="length">The length of the data in bytes.</param>
		/// <returns>The deserialized picture, or <see langword="null" /> if deserialization fails.</returns>
		/// <remarks />
		public static SKPicture Deserialize (IntPtr data, int length)
		{
			if (data == IntPtr.Zero)
				throw new ArgumentNullException (nameof (data));

			if (length == 0)
				return null;

			return GetObject (SkiaApi.sk_picture_deserialize_from_memory ((void*)data, (IntPtr)length));
		}

		/// <summary>Deserializes a picture from a byte span.</summary>
		/// <param name="data">The byte span containing the serialized picture.</param>
		/// <returns>The deserialized picture, or <see langword="null" /> if deserialization fails.</returns>
		/// <remarks />
		public static SKPicture Deserialize (ReadOnlySpan<byte> data)
		{
			if (data.Length == 0)
				return null;

			fixed (void* ptr = data) {
				return GetObject (SkiaApi.sk_picture_deserialize_from_memory (ptr, (IntPtr)data.Length));
			}
		}

		/// <summary>Deserializes a picture from serialized data.</summary>
		/// <param name="data">The SKData object containing the serialized picture.</param>
		/// <returns>The deserialized picture, or <see langword="null" /> if deserialization fails.</returns>
		/// <remarks />
		public static SKPicture Deserialize (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var picture = GetObject (SkiaApi.sk_picture_deserialize_from_data (data.Handle));
			GC.KeepAlive (data);
			return picture;
		}

		/// <summary>Deserializes a picture from a .NET stream.</summary>
		/// <param name="stream">The .NET stream containing the serialized picture.</param>
		/// <returns>The deserialized picture, or <see langword="null" /> if deserialization fails.</returns>
		/// <remarks />
		public static SKPicture Deserialize (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			using var managed = new SKManagedStream (stream);
			return Deserialize (managed);
		}

		/// <summary>Deserializes a picture from a stream.</summary>
		/// <param name="stream">The SKStream containing the serialized picture.</param>
		/// <returns>The deserialized picture, or <see langword="null" /> if deserialization fails.</returns>
		/// <remarks />
		public static SKPicture Deserialize (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var picture = GetObject (SkiaApi.sk_picture_deserialize_from_stream (stream.Handle));
			GC.KeepAlive (stream);
			return picture;
		}

		//

		internal static SKPicture GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKPicture (h, o));
	}
}
