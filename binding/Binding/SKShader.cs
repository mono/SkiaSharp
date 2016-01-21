//
// Bindings for SKShader
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKShader : IDisposable
	{
		internal IntPtr handle;

		SKShader (IntPtr handle)
		{
			this.handle = handle;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_shader_unref (handle);
				// We set this in case the user tries to use the fetched Canvas (which depends on us) to perform some operations
				handle = IntPtr.Zero;
			}
		}

		~SKShader()
		{
			Dispose (false);
		}

		public static SKShader CreateEmpty ()
		{
			return new SKShader (SkiaApi.sk_shader_new_empty ());
		}

		public static SKShader CreateColor (SKColor color)
		{
			return new SKShader (SkiaApi.sk_shader_new_color (color));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			return new SKShader (SkiaApi.sk_shader_new_bitmap (src.handle, tmx, tmy, IntPtr.Zero));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			return new SKShader (SkiaApi.sk_shader_new_bitmap (src.handle, tmx, tmy, ref localMatrix));
		}

		public static SKShader CreateLocalMatrix (SKShader shader)
		{
			return new SKShader (SkiaApi.sk_shader_new_local_matrix (shader.handle, IntPtr.Zero));
		}

		public static SKShader CreateLocalMatrix (SKShader shader, SKMatrix localMatrix)
		{
			return new SKShader (SkiaApi.sk_shader_new_local_matrix (shader.handle, ref localMatrix));
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			return new SKShader (SkiaApi.sk_shader_new_linear_gradient (new SKPoint []{start, end}, colors, colorPos, colors.Length, mode, IntPtr.Zero));
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			return new SKShader (SkiaApi.sk_shader_new_linear_gradient (new SKPoint []{start, end}, colors, colorPos, colors.Length, mode, ref localMatrix));
		}

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			return new SKShader (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, colorPos, colors.Length, mode, IntPtr.Zero));
		}
		
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			return new SKShader (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, colorPos, colors.Length, mode, ref localMatrix)); 
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos)
		{
			return new SKShader (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, colorPos, colors.Length, IntPtr.Zero));
		}
		
		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKMatrix localMatrix)
		{
				return new SKShader (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, colorPos, colors.Length, ref localMatrix));
		}

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			return new SKShader (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, colorPos, colors.Length, mode, IntPtr.Zero));
		}
		
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			return new SKShader (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, colorPos, colors.Length, mode, ref localMatrix));
		}
		
	}
}

