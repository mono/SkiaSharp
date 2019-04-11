namespace SkiaSharp
{
	export class SkiaApi
	{
		public static sk_colorspace_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_unref_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_unref(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_gamma_close_to_srgb_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_gamma_close_to_srgb_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_close_to_srgb_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_close_to_srgb_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_gamma_close_to_srgb(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_close_to_srgb_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_close_to_srgb_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_gamma_is_linear_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_gamma_is_linear_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_is_linear_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_is_linear_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_gamma_is_linear(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_is_linear_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_is_linear_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_is_srgb_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_is_srgb_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_is_srgb_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_is_srgb_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_is_srgb(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_is_srgb_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_is_srgb_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_gamma_get_type_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_gamma_get_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_type_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_gamma_get_type(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_gamma_get_gamma_named_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_gamma_get_gamma_named_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_gamma_named_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_gamma_named_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_gamma_get_gamma_named(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_gamma_named_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_gamma_get_gamma_named_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_equals_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_equals_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_equals_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_equals_0_Pre(parms);
			}
			var src = parms.src;
			var dst = parms.dst;
			var ret = CanvasKit._sk_colorspace_equals(src, dst);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_equals_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_equals_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_srgb_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_colorspace_new_srgb();
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_srgb_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_srgb_0_Post(ret);
			}
			return ret;
		}
		public static sk_colorspace_new_srgb_linear_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_colorspace_new_srgb_linear();
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_srgb_linear_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_srgb_linear_0_Post(ret);
			}
			return ret;
		}
		public static sk_colorspace_new_icc_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_icc_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_0_Pre(parms);
			}
			var input = parms.input;
			var len = parms.len;
			var ret = CanvasKit._sk_colorspace_new_icc(input, len);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_icc_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_icc_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_1_Pre(parms);
			}
			var input = CanvasKit._malloc(parms.input_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.input_Length; i++)
				{
					CanvasKit.HEAPU8[input + i] = parms.input[i];
				}
			}
			var len = parms.len;
			var ret = CanvasKit._sk_colorspace_new_icc(input, len);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_icc_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_rgb_with_gamma_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_rgb_with_gamma_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_0_Pre(parms);
			}
			var gamma = parms.gamma;
			var toXYZD50 = parms.toXYZD50;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_gamma(gamma, toXYZD50);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_rgb_with_gamma_and_gamut_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_rgb_with_gamma_and_gamut_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_and_gamut_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_and_gamut_0_Pre(parms);
			}
			var gamma = parms.gamma;
			var gamut = parms.gamut;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_gamma_and_gamut(gamma, gamut);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_and_gamut_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_and_gamut_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_rgb_with_coeffs_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspace_new_rgb_with_coeffs_0_Return();
			var parms = sk_colorspace_new_rgb_with_coeffs_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_0_Pre(parms);
			}
			var coeffs = parms.coeffs.marshalNew(CanvasKit);
			var toXYZD50 = parms.toXYZD50;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_coeffs(coeffs, toXYZD50);
			var retStruct = new sk_colorspace_new_rgb_with_coeffs_0_Return();
			retStruct.coeffs = SkiaSharp.SKColorSpaceTransferFn.unmarshal(coeffs, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colorspace_new_rgb_with_coeffs_and_gamut_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Return();
			var parms = sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Pre(parms);
			}
			var coeffs = parms.coeffs.marshalNew(CanvasKit);
			var gamut = parms.gamut;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_coeffs_and_gamut(coeffs, gamut);
			var retStruct = new sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Return();
			retStruct.coeffs = SkiaSharp.SKColorSpaceTransferFn.unmarshal(coeffs, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colorspace_new_rgb_with_gamma_named_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_rgb_with_gamma_named_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_0_Pre(parms);
			}
			var gamma = parms.gamma;
			var toXYZD50 = parms.toXYZD50;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_gamma_named(gamma, toXYZD50);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_new_rgb_with_gamma_named_and_gamut_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Pre(parms);
			}
			var gamma = parms.gamma;
			var gamut = parms.gamut;
			var ret = CanvasKit._sk_colorspace_new_rgb_with_gamma_named_and_gamut(gamma, gamut);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_to_xyzd50_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_to_xyzd50_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_to_xyzd50_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_to_xyzd50_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var toXYZD50 = parms.toXYZD50;
			var ret = CanvasKit._sk_colorspace_to_xyzd50(cColorSpace, toXYZD50);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_to_xyzd50_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_to_xyzd50_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_as_to_xyzd50_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_as_to_xyzd50_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_as_to_xyzd50_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_as_to_xyzd50_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_as_to_xyzd50(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_as_to_xyzd50_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_as_to_xyzd50_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_as_from_xyzd50_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorspace_as_from_xyzd50_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_as_from_xyzd50_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_as_from_xyzd50_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var ret = CanvasKit._sk_colorspace_as_from_xyzd50(cColorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_as_from_xyzd50_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_as_from_xyzd50_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorspace_is_numerical_transfer_fn_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspace_is_numerical_transfer_fn_0_Return();
			var parms = sk_colorspace_is_numerical_transfer_fn_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_is_numerical_transfer_fn_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_is_numerical_transfer_fn_0_Pre(parms);
			}
			var cColorSpace = parms.cColorSpace;
			var fn = retStruct.fn.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_colorspace_is_numerical_transfer_fn(cColorSpace, fn);
			var retStruct = new sk_colorspace_is_numerical_transfer_fn_0_Return();
			retStruct.fn = SkiaSharp.SKColorSpaceTransferFn.unmarshal(fn, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_is_numerical_transfer_fn_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_is_numerical_transfer_fn_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colorspaceprimaries_to_xyzd50_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspaceprimaries_to_xyzd50_0_Return();
			var parms = sk_colorspaceprimaries_to_xyzd50_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspaceprimaries_to_xyzd50_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspaceprimaries_to_xyzd50_0_Pre(parms);
			}
			var primaries = parms.primaries.marshalNew(CanvasKit);
			var toXYZD50 = parms.toXYZD50;
			var ret = CanvasKit._sk_colorspaceprimaries_to_xyzd50(primaries, toXYZD50);
			var retStruct = new sk_colorspaceprimaries_to_xyzd50_0_Return();
			retStruct.primaries = SkiaSharp.SKColorSpacePrimaries.unmarshal(primaries, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspaceprimaries_to_xyzd50_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspaceprimaries_to_xyzd50_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colorspace_transfer_fn_invert_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspace_transfer_fn_invert_0_Return();
			var parms = sk_colorspace_transfer_fn_invert_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_invert_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_invert_0_Pre(parms);
			}
			var transfer = parms.transfer.marshalNew(CanvasKit);
			var inverted = retStruct.inverted.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_colorspace_transfer_fn_invert(transfer, inverted);
			var retStruct = new sk_colorspace_transfer_fn_invert_0_Return();
			retStruct.transfer = SkiaSharp.SKColorSpaceTransferFn.unmarshal(transfer, CanvasKit);
			retStruct.inverted = SkiaSharp.SKColorSpaceTransferFn.unmarshal(inverted, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_invert_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_invert_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colorspace_transfer_fn_transform_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorspace_transfer_fn_transform_0_Return();
			var parms = sk_colorspace_transfer_fn_transform_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_transform_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_transform_0_Pre(parms);
			}
			var transfer = parms.transfer.marshalNew(CanvasKit);
			var x = parms.x;
			var ret = CanvasKit._sk_colorspace_transfer_fn_transform(transfer, x);
			var retStruct = new sk_colorspace_transfer_fn_transform_0_Return();
			retStruct.transfer = SkiaSharp.SKColorSpaceTransferFn.unmarshal(transfer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_transform_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorspace_transfer_fn_transform_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_colortype_get_default_8888_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_colortype_get_default_8888();
			if((<any>SkiaSharp.ApiOverride).sk_colortype_get_default_8888_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colortype_get_default_8888_0_Post(ret);
			}
			return ret;
		}
		public static sk_surface_new_null_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_new_null_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_null_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_null_0_Pre(parms);
			}
			var width = parms.width;
			var height = parms.height;
			var ret = CanvasKit._sk_surface_new_null(width, height);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_null_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_null_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_unref_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_surface_unref(t);
			if((<any>SkiaSharp.ApiOverride).sk_surface_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_raster_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_surface_new_raster_0_Return();
			var parms = sk_surface_new_raster_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_raster_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_raster_0_Pre(parms);
			}
			var info = parms.info.marshalNew(CanvasKit);
			var rowBytes = parms.rowBytes;
			var props = parms.props;
			var ret = CanvasKit._sk_surface_new_raster(info, rowBytes, props);
			var retStruct = new sk_surface_new_raster_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_raster_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_raster_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_surface_new_raster_direct_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_surface_new_raster_direct_0_Return();
			var parms = sk_surface_new_raster_direct_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_raster_direct_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_raster_direct_0_Pre(parms);
			}
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var releaseProc = parms.releaseProc;
			var context = parms.context;
			var props = parms.props;
			var ret = CanvasKit._sk_surface_new_raster_direct(info, pixels, rowBytes, releaseProc, context, props);
			var retStruct = new sk_surface_new_raster_direct_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_raster_direct_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_raster_direct_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_surface_get_canvas_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_get_canvas_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_get_canvas_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_get_canvas_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_surface_get_canvas(t);
			if((<any>SkiaSharp.ApiOverride).sk_surface_get_canvas_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_get_canvas_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_image_snapshot_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_new_image_snapshot_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_image_snapshot_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_image_snapshot_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_surface_new_image_snapshot(t);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_image_snapshot_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_image_snapshot_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_backend_render_target_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_new_backend_render_target_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_render_target_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_backend_render_target_0_Pre(parms);
			}
			var context = parms.context;
			var target = parms.target;
			var origin = parms.origin;
			var colorType = parms.colorType;
			var colorspace = parms.colorspace;
			var props = parms.props;
			var ret = CanvasKit._sk_surface_new_backend_render_target(context, target, origin, colorType, colorspace, props);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_render_target_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_backend_render_target_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_backend_texture_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_new_backend_texture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_0_Pre(parms);
			}
			var context = parms.context;
			var texture = parms.texture;
			var origin = parms.origin;
			var samples = parms.samples;
			var colorType = parms.colorType;
			var colorspace = parms.colorspace;
			var props = parms.props;
			var ret = CanvasKit._sk_surface_new_backend_texture(context, texture, origin, samples, colorType, colorspace, props);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_backend_texture_as_render_target_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_new_backend_texture_as_render_target_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_as_render_target_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_as_render_target_0_Pre(parms);
			}
			var context = parms.context;
			var texture = parms.texture;
			var origin = parms.origin;
			var samples = parms.samples;
			var colorType = parms.colorType;
			var colorspace = parms.colorspace;
			var props = parms.props;
			var ret = CanvasKit._sk_surface_new_backend_texture_as_render_target(context, texture, origin, samples, colorType, colorspace, props);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_as_render_target_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_backend_texture_as_render_target_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_new_render_target_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_surface_new_render_target_0_Return();
			var parms = sk_surface_new_render_target_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_render_target_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_new_render_target_0_Pre(parms);
			}
			var context = parms.context;
			var budgeted = parms.budgeted;
			var info = parms.info.marshalNew(CanvasKit);
			var sampleCount = parms.sampleCount;
			var origin = parms.origin;
			var props = parms.props;
			var shouldCreateWithMips = parms.shouldCreateWithMips;
			var ret = CanvasKit._sk_surface_new_render_target(context, budgeted, info, sampleCount, origin, props, shouldCreateWithMips);
			var retStruct = new sk_surface_new_render_target_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_surface_new_render_target_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_new_render_target_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_surface_draw_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_draw_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_draw_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_draw_0_Pre(parms);
			}
			var surface = parms.surface;
			var canvas = parms.canvas;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_surface_draw(surface, canvas, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_surface_draw_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_draw_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_peek_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_peek_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_peek_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_peek_pixels_0_Pre(parms);
			}
			var surface = parms.surface;
			var pixmap = parms.pixmap;
			var ret = CanvasKit._sk_surface_peek_pixels(surface, pixmap);
			if((<any>SkiaSharp.ApiOverride).sk_surface_peek_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_peek_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surface_read_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_surface_read_pixels_0_Return();
			var parms = sk_surface_read_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_read_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_read_pixels_0_Pre(parms);
			}
			var surface = parms.surface;
			var dstInfo = parms.dstInfo.marshalNew(CanvasKit);
			var dstPixels = parms.dstPixels;
			var dstRowBytes = parms.dstRowBytes;
			var srcX = parms.srcX;
			var srcY = parms.srcY;
			var ret = CanvasKit._sk_surface_read_pixels(surface, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
			var retStruct = new sk_surface_read_pixels_0_Return();
			retStruct.dstInfo = SkiaSharp.SKImageInfoNative.unmarshal(dstInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_surface_read_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_read_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_surface_get_props_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surface_get_props_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surface_get_props_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surface_get_props_0_Pre(parms);
			}
			var surface = parms.surface;
			var ret = CanvasKit._sk_surface_get_props(surface);
			if((<any>SkiaSharp.ApiOverride).sk_surface_get_props_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surface_get_props_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surfaceprops_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surfaceprops_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surfaceprops_new_0_Pre(parms);
			}
			var flags = parms.flags;
			var geometry = parms.geometry;
			var ret = CanvasKit._sk_surfaceprops_new(flags, geometry);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surfaceprops_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surfaceprops_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surfaceprops_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surfaceprops_delete_0_Pre(parms);
			}
			var props = parms.props;
			var ret = CanvasKit._sk_surfaceprops_delete(props);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surfaceprops_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surfaceprops_get_flags_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surfaceprops_get_flags_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_flags_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_flags_0_Pre(parms);
			}
			var props = parms.props;
			var ret = CanvasKit._sk_surfaceprops_get_flags(props);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_flags_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_flags_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_surfaceprops_get_pixel_geometry_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_surfaceprops_get_pixel_geometry_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_pixel_geometry_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_pixel_geometry_0_Pre(parms);
			}
			var props = parms.props;
			var ret = CanvasKit._sk_surfaceprops_get_pixel_geometry(props);
			if((<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_pixel_geometry_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_surfaceprops_get_pixel_geometry_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_save_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_save_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_save_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_canvas_save(t);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_save_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_save_layer_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_save_layer_0_Return();
			var parms = sk_canvas_save_layer_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_save_layer(t, rect, paint);
			var retStruct = new sk_canvas_save_layer_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_save_layer_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_save_layer_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_1_Pre(parms);
			}
			var t = parms.t;
			var rectZero = parms.rectZero;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_save_layer(t, rectZero, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_save_layer_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_restore_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_restore_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_restore_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_restore_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_canvas_restore(t);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_restore_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_restore_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_get_save_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_get_save_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_save_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_get_save_count_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_canvas_get_save_count(t);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_save_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_get_save_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_restore_to_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_restore_to_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_restore_to_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_restore_to_count_0_Pre(parms);
			}
			var t = parms.t;
			var saveCount = parms.saveCount;
			var ret = CanvasKit._sk_canvas_restore_to_count(t, saveCount);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_restore_to_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_restore_to_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_translate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_translate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_translate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_translate_0_Pre(parms);
			}
			var t = parms.t;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_canvas_translate(t, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_translate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_translate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_scale_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_scale_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_scale_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_scale_0_Pre(parms);
			}
			var t = parms.t;
			var sx = parms.sx;
			var sy = parms.sy;
			var ret = CanvasKit._sk_canvas_scale(t, sx, sy);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_scale_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_scale_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_rotate_degrees_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_rotate_degrees_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_rotate_degrees_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_rotate_degrees_0_Pre(parms);
			}
			var t = parms.t;
			var degrees = parms.degrees;
			var ret = CanvasKit._sk_canvas_rotate_degrees(t, degrees);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_rotate_degrees_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_rotate_degrees_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_rotate_radians_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_rotate_radians_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_rotate_radians_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_rotate_radians_0_Pre(parms);
			}
			var t = parms.t;
			var radians = parms.radians;
			var ret = CanvasKit._sk_canvas_rotate_radians(t, radians);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_rotate_radians_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_rotate_radians_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_skew_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_skew_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_skew_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_skew_0_Pre(parms);
			}
			var t = parms.t;
			var sx = parms.sx;
			var sy = parms.sy;
			var ret = CanvasKit._sk_canvas_skew(t, sx, sy);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_skew_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_skew_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_concat_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_concat_0_Return();
			var parms = sk_canvas_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_concat_0_Pre(parms);
			}
			var t = parms.t;
			var m = parms.m.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_concat(t, m);
			var retStruct = new sk_canvas_concat_0_Return();
			retStruct.m = SkiaSharp.SKMatrix.unmarshal(m, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_concat_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_quick_reject_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_quick_reject_0_Return();
			var parms = sk_canvas_quick_reject_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_quick_reject_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_quick_reject_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_quick_reject(t, rect);
			var retStruct = new sk_canvas_quick_reject_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_quick_reject_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_quick_reject_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_paint_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_paint_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_paint_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_paint_0_Pre(parms);
			}
			var t = parms.t;
			var p = parms.p;
			var ret = CanvasKit._sk_canvas_draw_paint(t, p);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_paint_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_paint_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_region_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_region_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_region_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_region_0_Pre(parms);
			}
			var t = parms.t;
			var region = parms.region;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_region(t, region, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_region_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_region_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_rect_0_Return();
			var parms = sk_canvas_draw_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_rect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_rect(t, rect, paint);
			var retStruct = new sk_canvas_draw_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_rrect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_rrect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_rrect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_rrect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_rrect(t, rect, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_rrect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_rrect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_round_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_round_rect_0_Return();
			var parms = sk_canvas_draw_round_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_round_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_round_rect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var rx = parms.rx;
			var ry = parms.ry;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_round_rect(t, rect, rx, ry, paint);
			var retStruct = new sk_canvas_draw_round_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_round_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_round_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_oval_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_oval_0_Return();
			var parms = sk_canvas_draw_oval_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_oval_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_oval_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_oval(t, rect, paint);
			var retStruct = new sk_canvas_draw_oval_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_oval_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_oval_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_circle_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_circle_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_circle_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_circle_0_Pre(parms);
			}
			var t = parms.t;
			var cx = parms.cx;
			var cy = parms.cy;
			var radius = parms.radius;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_circle(t, cx, cy, radius, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_circle_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_circle_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_path_0_Pre(parms);
			}
			var t = parms.t;
			var path = parms.path;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_path(t, path, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_image_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_image_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_0_Pre(parms);
			}
			var t = parms.t;
			var image = parms.image;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_image(t, image, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_image_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_image_rect_0_Return();
			var parms = sk_canvas_draw_image_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_0_Pre(parms);
			}
			var t = parms.t;
			var image = parms.image;
			var src = parms.src.marshalNew(CanvasKit);
			var dest = parms.dest.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_image_rect(t, image, src, dest, paint);
			var retStruct = new sk_canvas_draw_image_rect_0_Return();
			retStruct.src = SkiaSharp.SKRect.unmarshal(src, CanvasKit);
			retStruct.dest = SkiaSharp.SKRect.unmarshal(dest, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_image_rect_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_image_rect_1_Return();
			var parms = sk_canvas_draw_image_rect_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_1_Pre(parms);
			}
			var t = parms.t;
			var image = parms.image;
			var srcZero = parms.srcZero;
			var dest = parms.dest.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_image_rect(t, image, srcZero, dest, paint);
			var retStruct = new sk_canvas_draw_image_rect_1_Return();
			retStruct.dest = SkiaSharp.SKRect.unmarshal(dest, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_rect_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_picture_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_picture_0_Return();
			var parms = sk_canvas_draw_picture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_0_Pre(parms);
			}
			var t = parms.t;
			var pict = parms.pict;
			var mat = parms.mat.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_picture(t, pict, mat, paint);
			var retStruct = new sk_canvas_draw_picture_0_Return();
			retStruct.mat = SkiaSharp.SKMatrix.unmarshal(mat, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_picture_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_picture_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_1_Pre(parms);
			}
			var t = parms.t;
			var pict = parms.pict;
			var matZero = parms.matZero;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_picture(t, pict, matZero, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_picture_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_drawable_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_drawable_0_Return();
			var parms = sk_canvas_draw_drawable_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_drawable_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_drawable_0_Pre(parms);
			}
			var t = parms.t;
			var drawable = parms.drawable;
			var mat = parms.mat.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_draw_drawable(t, drawable, mat);
			var retStruct = new sk_canvas_draw_drawable_0_Return();
			retStruct.mat = SkiaSharp.SKMatrix.unmarshal(mat, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_drawable_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_drawable_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_color_0_Pre(parms);
			}
			var t = parms.t;
			var color = parms.color.color;
			var mode = parms.mode;
			var ret = CanvasKit._sk_canvas_draw_color(t, color, mode);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_points_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_points_0_Pre(parms);
			}
			var t = parms.t;
			var mode = parms.mode;
			var count = parms.count;
			var points = parms.points;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_points(t, mode, count, points, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_points_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_point_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_point_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_point_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_point_0_Pre(parms);
			}
			var t = parms.t;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_point(t, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_point_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_point_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_line_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_line_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_line_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_line_0_Pre(parms);
			}
			var t = parms.t;
			var x0 = parms.x0;
			var y0 = parms.y0;
			var x1 = parms.x1;
			var y1 = parms.y1;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_line(t, x0, y0, x1, y1, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_line_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_line_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_0_Pre(parms);
			}
			var t = parms.t;
			var text = CanvasKit._malloc(parms.text_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.text_Length; i++)
				{
					CanvasKit.HEAPU8[text + i] = parms.text[i];
				}
			}
			var len = parms.len;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_text(t, text, len, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_text_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_text_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_1_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var len = parms.len;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_text(t, text, len, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_pos_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_pos_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_0_Pre(parms);
			}
			var t = parms.t;
			var text = CanvasKit._malloc(parms.text_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.text_Length; i++)
				{
					CanvasKit.HEAPU8[text + i] = parms.text[i];
				}
			}
			var len = parms.len;
			var points = parms.points;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_pos_text(t, text, len, points, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_pos_text_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_pos_text_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_1_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var len = parms.len;
			var points = parms.points;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_pos_text(t, text, len, points, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_pos_text_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_text_on_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_text_on_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_0_Pre(parms);
			}
			var t = parms.t;
			var text = CanvasKit._malloc(parms.text_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.text_Length; i++)
				{
					CanvasKit.HEAPU8[text + i] = parms.text[i];
				}
			}
			var len = parms.len;
			var path = parms.path;
			var hOffset = parms.hOffset;
			var vOffset = parms.vOffset;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_text_on_path(t, text, len, path, hOffset, vOffset, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_text_on_path_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_text_on_path_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_1_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var len = parms.len;
			var path = parms.path;
			var hOffset = parms.hOffset;
			var vOffset = parms.vOffset;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_text_on_path(t, text, len, path, hOffset, vOffset, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_on_path_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_text_blob_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_text_blob_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_blob_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_blob_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var text = parms.text;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_text_blob(canvas, text, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_blob_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_text_blob_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_bitmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_bitmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_0_Pre(parms);
			}
			var t = parms.t;
			var bitmap = parms.bitmap;
			var x = parms.x;
			var y = parms.y;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_bitmap(t, bitmap, x, y, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_bitmap_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_bitmap_rect_0_Return();
			var parms = sk_canvas_draw_bitmap_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_0_Pre(parms);
			}
			var t = parms.t;
			var bitmap = parms.bitmap;
			var src = parms.src.marshalNew(CanvasKit);
			var dest = parms.dest.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_bitmap_rect(t, bitmap, src, dest, paint);
			var retStruct = new sk_canvas_draw_bitmap_rect_0_Return();
			retStruct.src = SkiaSharp.SKRect.unmarshal(src, CanvasKit);
			retStruct.dest = SkiaSharp.SKRect.unmarshal(dest, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_bitmap_rect_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_bitmap_rect_1_Return();
			var parms = sk_canvas_draw_bitmap_rect_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_1_Pre(parms);
			}
			var t = parms.t;
			var bitmap = parms.bitmap;
			var srcZero = parms.srcZero;
			var dest = parms.dest.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_bitmap_rect(t, bitmap, srcZero, dest, paint);
			var retStruct = new sk_canvas_draw_bitmap_rect_1_Return();
			retStruct.dest = SkiaSharp.SKRect.unmarshal(dest, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_rect_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_reset_matrix_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_reset_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_reset_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_reset_matrix_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_canvas_reset_matrix(canvas);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_reset_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_reset_matrix_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_set_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_set_matrix_0_Return();
			var parms = sk_canvas_set_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_set_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_set_matrix_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_set_matrix(canvas, matrix);
			var retStruct = new sk_canvas_set_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_set_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_set_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_get_total_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_get_total_matrix_0_Return();
			var parms = sk_canvas_get_total_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_total_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_get_total_matrix_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_get_total_matrix(canvas, matrix);
			var retStruct = new sk_canvas_get_total_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_total_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_get_total_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_annotation_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_annotation_0_Return();
			var parms = sk_canvas_draw_annotation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_annotation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_annotation_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var key = CanvasKit._malloc(parms.key_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.key_Length; i++)
				{
					CanvasKit.HEAPU8[key + i] = parms.key[i];
				}
			}
			var value = parms.value;
			var ret = CanvasKit._sk_canvas_draw_annotation(t, rect, key, value);
			var retStruct = new sk_canvas_draw_annotation_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_annotation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_annotation_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_url_annotation_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_url_annotation_0_Return();
			var parms = sk_canvas_draw_url_annotation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_url_annotation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_url_annotation_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var value = parms.value;
			var ret = CanvasKit._sk_canvas_draw_url_annotation(t, rect, value);
			var retStruct = new sk_canvas_draw_url_annotation_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_url_annotation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_url_annotation_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_named_destination_annotation_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_named_destination_annotation_0_Return();
			var parms = sk_canvas_draw_named_destination_annotation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_named_destination_annotation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_named_destination_annotation_0_Pre(parms);
			}
			var t = parms.t;
			var point = parms.point.marshalNew(CanvasKit);
			var value = parms.value;
			var ret = CanvasKit._sk_canvas_draw_named_destination_annotation(t, point, value);
			var retStruct = new sk_canvas_draw_named_destination_annotation_0_Return();
			retStruct.point = SkiaSharp.SKPoint.unmarshal(point, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_named_destination_annotation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_named_destination_annotation_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_link_destination_annotation_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_link_destination_annotation_0_Return();
			var parms = sk_canvas_draw_link_destination_annotation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_link_destination_annotation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_link_destination_annotation_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var value = parms.value;
			var ret = CanvasKit._sk_canvas_draw_link_destination_annotation(t, rect, value);
			var retStruct = new sk_canvas_draw_link_destination_annotation_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_link_destination_annotation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_link_destination_annotation_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_clip_rrect_with_operation_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_clip_rrect_with_operation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_rrect_with_operation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_clip_rrect_with_operation_0_Pre(parms);
			}
			var t = parms.t;
			var crect = parms.crect;
			var op = parms.op;
			var doAA = parms.doAA;
			var ret = CanvasKit._sk_canvas_clip_rrect_with_operation(t, crect, op, doAA);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_rrect_with_operation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_clip_rrect_with_operation_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_clip_rect_with_operation_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_clip_rect_with_operation_0_Return();
			var parms = sk_canvas_clip_rect_with_operation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_rect_with_operation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_clip_rect_with_operation_0_Pre(parms);
			}
			var t = parms.t;
			var crect = parms.crect.marshalNew(CanvasKit);
			var op = parms.op;
			var doAA = parms.doAA;
			var ret = CanvasKit._sk_canvas_clip_rect_with_operation(t, crect, op, doAA);
			var retStruct = new sk_canvas_clip_rect_with_operation_0_Return();
			retStruct.crect = SkiaSharp.SKRect.unmarshal(crect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_rect_with_operation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_clip_rect_with_operation_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_clip_path_with_operation_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_clip_path_with_operation_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_path_with_operation_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_clip_path_with_operation_0_Pre(parms);
			}
			var t = parms.t;
			var cpath = parms.cpath;
			var op = parms.op;
			var doAA = parms.doAA;
			var ret = CanvasKit._sk_canvas_clip_path_with_operation(t, cpath, op, doAA);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_path_with_operation_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_clip_path_with_operation_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_clip_region_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_clip_region_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_region_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_clip_region_0_Pre(parms);
			}
			var t = parms.t;
			var region = parms.region;
			var op = parms.op;
			var ret = CanvasKit._sk_canvas_clip_region(t, region, op);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_clip_region_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_clip_region_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_get_device_clip_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_get_device_clip_bounds_0_Return();
			var parms = sk_canvas_get_device_clip_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_device_clip_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_get_device_clip_bounds_0_Pre(parms);
			}
			var t = parms.t;
			var cbounds = retStruct.cbounds.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_get_device_clip_bounds(t, cbounds);
			var retStruct = new sk_canvas_get_device_clip_bounds_0_Return();
			retStruct.cbounds = SkiaSharp.SKRectI.unmarshal(cbounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_device_clip_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_get_device_clip_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_get_local_clip_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_get_local_clip_bounds_0_Return();
			var parms = sk_canvas_get_local_clip_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_local_clip_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_get_local_clip_bounds_0_Pre(parms);
			}
			var t = parms.t;
			var cbounds = retStruct.cbounds.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_canvas_get_local_clip_bounds(t, cbounds);
			var retStruct = new sk_canvas_get_local_clip_bounds_0_Return();
			retStruct.cbounds = SkiaSharp.SKRect.unmarshal(cbounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_get_local_clip_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_get_local_clip_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_new_from_bitmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_new_from_bitmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_new_from_bitmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_new_from_bitmap_0_Pre(parms);
			}
			var bitmap = parms.bitmap;
			var ret = CanvasKit._sk_canvas_new_from_bitmap(bitmap);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_new_from_bitmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_new_from_bitmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_flush_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_flush_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_flush_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_flush_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_canvas_flush(canvas);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_flush_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_flush_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_bitmap_lattice_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_bitmap_lattice_0_Return();
			var parms = sk_canvas_draw_bitmap_lattice_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_lattice_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_lattice_0_Pre(parms);
			}
			var t = parms.t;
			var bitmap = parms.bitmap;
			var lattice = parms.lattice.marshalNew(CanvasKit);
			var dst = parms.dst.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_bitmap_lattice(t, bitmap, lattice, dst, paint);
			var retStruct = new sk_canvas_draw_bitmap_lattice_0_Return();
			retStruct.lattice = SkiaSharp.SKLatticeInternal.unmarshal(lattice, CanvasKit);
			retStruct.dst = SkiaSharp.SKRect.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_lattice_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_lattice_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_image_lattice_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_image_lattice_0_Return();
			var parms = sk_canvas_draw_image_lattice_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_lattice_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_lattice_0_Pre(parms);
			}
			var t = parms.t;
			var image = parms.image;
			var lattice = parms.lattice.marshalNew(CanvasKit);
			var dst = parms.dst.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_image_lattice(t, image, lattice, dst, paint);
			var retStruct = new sk_canvas_draw_image_lattice_0_Return();
			retStruct.lattice = SkiaSharp.SKLatticeInternal.unmarshal(lattice, CanvasKit);
			retStruct.dst = SkiaSharp.SKRect.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_lattice_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_lattice_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_bitmap_nine_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_bitmap_nine_0_Return();
			var parms = sk_canvas_draw_bitmap_nine_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_nine_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_nine_0_Pre(parms);
			}
			var t = parms.t;
			var bitmap = parms.bitmap;
			var center = parms.center.marshalNew(CanvasKit);
			var dst = parms.dst.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_bitmap_nine(t, bitmap, center, dst, paint);
			var retStruct = new sk_canvas_draw_bitmap_nine_0_Return();
			retStruct.center = SkiaSharp.SKRectI.unmarshal(center, CanvasKit);
			retStruct.dst = SkiaSharp.SKRect.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_nine_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_bitmap_nine_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_draw_image_nine_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_canvas_draw_image_nine_0_Return();
			var parms = sk_canvas_draw_image_nine_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_nine_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_nine_0_Pre(parms);
			}
			var t = parms.t;
			var image = parms.image;
			var center = parms.center.marshalNew(CanvasKit);
			var dst = parms.dst.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_image_nine(t, image, center, dst, paint);
			var retStruct = new sk_canvas_draw_image_nine_0_Return();
			retStruct.center = SkiaSharp.SKRectI.unmarshal(center, CanvasKit);
			retStruct.dst = SkiaSharp.SKRect.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_nine_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_image_nine_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_canvas_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_destroy_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_canvas_destroy(canvas);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_canvas_draw_vertices_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_canvas_draw_vertices_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_vertices_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_canvas_draw_vertices_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var vertices = parms.vertices;
			var mode = parms.mode;
			var paint = parms.paint;
			var ret = CanvasKit._sk_canvas_draw_vertices(canvas, vertices, mode, paint);
			if((<any>SkiaSharp.ApiOverride).sk_canvas_draw_vertices_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_canvas_draw_vertices_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nodraw_canvas_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nodraw_canvas_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_new_0_Pre(parms);
			}
			var width = parms.width;
			var height = parms.height;
			var ret = CanvasKit._sk_nodraw_canvas_new(width, height);
			if((<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nodraw_canvas_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nodraw_canvas_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_destroy_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_nodraw_canvas_destroy(t);
			if((<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nodraw_canvas_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nway_canvas_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nway_canvas_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nway_canvas_new_0_Pre(parms);
			}
			var width = parms.width;
			var height = parms.height;
			var ret = CanvasKit._sk_nway_canvas_new(width, height);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nway_canvas_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nway_canvas_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nway_canvas_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nway_canvas_destroy_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_nway_canvas_destroy(t);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nway_canvas_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nway_canvas_add_canvas_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nway_canvas_add_canvas_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_add_canvas_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nway_canvas_add_canvas_0_Pre(parms);
			}
			var t = parms.t;
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_nway_canvas_add_canvas(t, canvas);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_add_canvas_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nway_canvas_add_canvas_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nway_canvas_remove_canvas_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nway_canvas_remove_canvas_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_canvas_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_canvas_0_Pre(parms);
			}
			var t = parms.t;
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_nway_canvas_remove_canvas(t, canvas);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_canvas_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_canvas_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_nway_canvas_remove_all_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_nway_canvas_remove_all_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_all_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_all_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_nway_canvas_remove_all(t);
			if((<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_all_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_nway_canvas_remove_all_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_overdraw_canvas_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_overdraw_canvas_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_new_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_overdraw_canvas_new(canvas);
			if((<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_overdraw_canvas_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_overdraw_canvas_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_destroy_0_Pre(parms);
			}
			var canvas = parms.canvas;
			var ret = CanvasKit._sk_overdraw_canvas_destroy(canvas);
			if((<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_overdraw_canvas_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_paint_new();
			if((<any>SkiaSharp.ApiOverride).sk_paint_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_paint_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_delete_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_delete(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_reset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_reset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_reset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_reset_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_reset(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_reset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_reset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_antialias_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_antialias_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_antialias_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_antialias_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_is_antialias(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_antialias_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_antialias_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_antialias_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_antialias_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_antialias_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_antialias_0_Pre(parms);
			}
			var t = parms.t;
			var v = parms.v;
			var ret = CanvasKit._sk_paint_set_antialias(t, v);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_antialias_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_antialias_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_dither_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_dither_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_dither_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_dither_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_is_dither(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_dither_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_dither_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_dither_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_dither_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_dither_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_dither_0_Pre(parms);
			}
			var t = parms.t;
			var v = parms.v;
			var ret = CanvasKit._sk_paint_set_dither(t, v);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_dither_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_dither_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_verticaltext_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_verticaltext_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_verticaltext_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_verticaltext_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_is_verticaltext(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_verticaltext_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_verticaltext_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_verticaltext_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_verticaltext_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_verticaltext_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_verticaltext_0_Pre(parms);
			}
			var t = parms.t;
			var v = parms.v;
			var ret = CanvasKit._sk_paint_set_verticaltext(t, v);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_verticaltext_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_verticaltext_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_color_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_color(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_color_0_Pre(parms);
			}
			var t = parms.t;
			var color = parms.color.color;
			var ret = CanvasKit._sk_paint_set_color(t, color);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_style_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_style(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_style_0_Pre(parms);
			}
			var t = parms.t;
			var style = parms.style;
			var ret = CanvasKit._sk_paint_set_style(t, style);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_stroke_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_stroke_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_width_0_Pre(parms);
			}
			var paint = parms.paint;
			var ret = CanvasKit._sk_paint_get_stroke_width(paint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_stroke_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_stroke_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_width_0_Pre(parms);
			}
			var t = parms.t;
			var width = parms.width;
			var ret = CanvasKit._sk_paint_set_stroke_width(t, width);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_stroke_miter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_stroke_miter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_miter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_miter_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_stroke_miter(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_miter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_miter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_stroke_miter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_stroke_miter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_miter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_miter_0_Pre(parms);
			}
			var t = parms.t;
			var miter = parms.miter;
			var ret = CanvasKit._sk_paint_set_stroke_miter(t, miter);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_miter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_miter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_stroke_cap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_stroke_cap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_cap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_cap_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_stroke_cap(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_cap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_cap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_stroke_cap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_stroke_cap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_cap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_cap_0_Pre(parms);
			}
			var t = parms.t;
			var cap = parms.cap;
			var ret = CanvasKit._sk_paint_set_stroke_cap(t, cap);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_cap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_cap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_stroke_join_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_stroke_join_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_join_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_join_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_stroke_join(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_join_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_stroke_join_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_stroke_join_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_stroke_join_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_join_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_join_0_Pre(parms);
			}
			var t = parms.t;
			var join = parms.join;
			var ret = CanvasKit._sk_paint_set_stroke_join(t, join);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_join_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_stroke_join_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_shader_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_shader_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_shader_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_shader_0_Pre(parms);
			}
			var t = parms.t;
			var shader = parms.shader;
			var ret = CanvasKit._sk_paint_set_shader(t, shader);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_shader_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_shader_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_shader_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_shader_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_shader_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_shader_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_shader(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_shader_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_shader_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_maskfilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_maskfilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_maskfilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_maskfilter_0_Pre(parms);
			}
			var t = parms.t;
			var filter = parms.filter;
			var ret = CanvasKit._sk_paint_set_maskfilter(t, filter);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_maskfilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_maskfilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_maskfilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_maskfilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_maskfilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_maskfilter_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_maskfilter(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_maskfilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_maskfilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_colorfilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_colorfilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_colorfilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_colorfilter_0_Pre(parms);
			}
			var t = parms.t;
			var filter = parms.filter;
			var ret = CanvasKit._sk_paint_set_colorfilter(t, filter);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_colorfilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_colorfilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_colorfilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_colorfilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_colorfilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_colorfilter_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_colorfilter(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_colorfilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_colorfilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_imagefilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_imagefilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_imagefilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_imagefilter_0_Pre(parms);
			}
			var t = parms.t;
			var filter = parms.filter;
			var ret = CanvasKit._sk_paint_set_imagefilter(t, filter);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_imagefilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_imagefilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_imagefilter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_imagefilter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_imagefilter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_imagefilter_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_imagefilter(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_imagefilter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_imagefilter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_blendmode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_blendmode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_blendmode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_blendmode_0_Pre(parms);
			}
			var t = parms.t;
			var mode = parms.mode;
			var ret = CanvasKit._sk_paint_set_blendmode(t, mode);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_blendmode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_blendmode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_blendmode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_blendmode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_blendmode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_blendmode_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_blendmode(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_blendmode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_blendmode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_filter_quality_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_filter_quality_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_filter_quality_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_filter_quality_0_Pre(parms);
			}
			var t = parms.t;
			var filterQuality = parms.filterQuality;
			var ret = CanvasKit._sk_paint_set_filter_quality(t, filterQuality);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_filter_quality_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_filter_quality_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_filter_quality_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_filter_quality_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_filter_quality_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_filter_quality_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_filter_quality(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_filter_quality_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_filter_quality_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_typeface_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_typeface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_typeface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_typeface_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_typeface(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_typeface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_typeface_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_typeface_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_typeface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_typeface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_typeface_0_Pre(parms);
			}
			var t = parms.t;
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_paint_set_typeface(t, typeface);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_typeface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_typeface_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_textsize_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_textsize_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_textsize_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_textsize_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_textsize(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_textsize_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_textsize_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_textsize_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_textsize_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_textsize_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_textsize_0_Pre(parms);
			}
			var t = parms.t;
			var size = parms.size;
			var ret = CanvasKit._sk_paint_set_textsize(t, size);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_textsize_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_textsize_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_align_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_align_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_align_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_align_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_text_align(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_align_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_align_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_text_align_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_text_align_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_align_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_text_align_0_Pre(parms);
			}
			var t = parms.t;
			var align = parms.align;
			var ret = CanvasKit._sk_paint_set_text_align(t, align);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_align_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_text_align_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_encoding_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_encoding_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_encoding_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_encoding_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_text_encoding(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_encoding_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_encoding_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_text_encoding_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_text_encoding_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_encoding_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_text_encoding_0_Pre(parms);
			}
			var t = parms.t;
			var encoding = parms.encoding;
			var ret = CanvasKit._sk_paint_set_text_encoding(t, encoding);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_encoding_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_text_encoding_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_scale_x_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_scale_x_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_scale_x_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_scale_x_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_text_scale_x(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_scale_x_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_scale_x_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_text_scale_x_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_text_scale_x_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_scale_x_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_text_scale_x_0_Pre(parms);
			}
			var t = parms.t;
			var scale = parms.scale;
			var ret = CanvasKit._sk_paint_set_text_scale_x(t, scale);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_scale_x_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_text_scale_x_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_skew_x_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_skew_x_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_skew_x_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_skew_x_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_paint_get_text_skew_x(t);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_skew_x_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_skew_x_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_text_skew_x_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_text_skew_x_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_skew_x_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_text_skew_x_0_Pre(parms);
			}
			var t = parms.t;
			var skew = parms.skew;
			var ret = CanvasKit._sk_paint_set_text_skew_x(t, skew);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_text_skew_x_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_text_skew_x_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_measure_text_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_paint_measure_text_0_Return();
			var parms = sk_paint_measure_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_measure_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_measure_text_0_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var length = parms.length;
			var bounds = parms.bounds.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_paint_measure_text(t, text, length, bounds);
			var retStruct = new sk_paint_measure_text_0_Return();
			retStruct.bounds = SkiaSharp.SKRect.unmarshal(bounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_paint_measure_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_measure_text_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_paint_measure_text_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_measure_text_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_measure_text_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_measure_text_1_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var length = parms.length;
			var boundsZero = parms.boundsZero;
			var ret = CanvasKit._sk_paint_measure_text(t, text, length, boundsZero);
			if((<any>SkiaSharp.ApiOverride).sk_paint_measure_text_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_measure_text_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_break_text_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_paint_break_text_0_Return();
			var parms = sk_paint_break_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_break_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_break_text_0_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var length = parms.length;
			var maxWidth = parms.maxWidth;
			var measuredWidth = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_paint_break_text(t, text, length, maxWidth, measuredWidth);
			var retStruct = new sk_paint_break_text_0_Return();
			retStruct.measuredWidth = CanvasKit.getValue(measuredWidth, "float");
			if((<any>SkiaSharp.ApiOverride).sk_paint_break_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_break_text_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_paint_get_text_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_path_0_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var length = parms.length;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_paint_get_text_path(t, text, length, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_pos_text_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_pos_text_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_path_0_Pre(parms);
			}
			var t = parms.t;
			var text = parms.text;
			var length = parms.length;
			var points = parms.points;
			var ret = CanvasKit._sk_paint_get_pos_text_path(t, text, length, points);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_fontmetrics_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_paint_get_fontmetrics_0_Return();
			var parms = sk_paint_get_fontmetrics_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_0_Pre(parms);
			}
			var t = parms.t;
			var fontMetrics = retStruct.fontMetrics.marshalNew(CanvasKit);
			var scale = parms.scale;
			var ret = CanvasKit._sk_paint_get_fontmetrics(t, fontMetrics, scale);
			var retStruct = new sk_paint_get_fontmetrics_0_Return();
			retStruct.fontMetrics = SkiaSharp.SKFontMetrics.unmarshal(fontMetrics, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_paint_get_fontmetrics_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_fontmetrics_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_1_Pre(parms);
			}
			var t = parms.t;
			var fontMetricsZero = parms.fontMetricsZero;
			var scale = parms.scale;
			var ret = CanvasKit._sk_paint_get_fontmetrics(t, fontMetricsZero, scale);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_fontmetrics_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_path_effect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_path_effect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_path_effect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_path_effect_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_get_path_effect(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_path_effect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_path_effect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_path_effect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_path_effect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_path_effect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_path_effect_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var effect = parms.effect;
			var ret = CanvasKit._sk_paint_set_path_effect(cpaint, effect);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_path_effect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_path_effect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_linear_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_linear_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_linear_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_linear_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_linear_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_linear_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_linear_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_linear_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_linear_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_linear_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_linear_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var linearText = parms.linearText;
			var ret = CanvasKit._sk_paint_set_linear_text(cpaint, linearText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_linear_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_linear_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_subpixel_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_subpixel_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_subpixel_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_subpixel_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_subpixel_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_subpixel_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_subpixel_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_subpixel_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_subpixel_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_subpixel_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_subpixel_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var subpixelText = parms.subpixelText;
			var ret = CanvasKit._sk_paint_set_subpixel_text(cpaint, subpixelText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_subpixel_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_subpixel_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_lcd_render_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_lcd_render_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_lcd_render_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_lcd_render_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_lcd_render_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_lcd_render_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_lcd_render_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_lcd_render_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_lcd_render_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_lcd_render_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_lcd_render_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var lcdText = parms.lcdText;
			var ret = CanvasKit._sk_paint_set_lcd_render_text(cpaint, lcdText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_lcd_render_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_lcd_render_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_embedded_bitmap_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_embedded_bitmap_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_embedded_bitmap_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_embedded_bitmap_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_embedded_bitmap_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_embedded_bitmap_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_embedded_bitmap_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_embedded_bitmap_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_embedded_bitmap_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_embedded_bitmap_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_embedded_bitmap_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var useEmbeddedBitmapText = parms.useEmbeddedBitmapText;
			var ret = CanvasKit._sk_paint_set_embedded_bitmap_text(cpaint, useEmbeddedBitmapText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_embedded_bitmap_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_embedded_bitmap_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_autohinted_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_autohinted_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_autohinted_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_autohinted_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_autohinted(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_autohinted_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_autohinted_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_autohinted_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_autohinted_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_autohinted_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_autohinted_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var useAutohinter = parms.useAutohinter;
			var ret = CanvasKit._sk_paint_set_autohinted(cpaint, useAutohinter);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_autohinted_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_autohinted_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_hinting_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_hinting_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_hinting_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_hinting_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_get_hinting(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_hinting_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_hinting_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_hinting_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_hinting_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_hinting_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_hinting_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var hintingLevel = parms.hintingLevel;
			var ret = CanvasKit._sk_paint_set_hinting(cpaint, hintingLevel);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_hinting_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_hinting_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_fake_bold_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_fake_bold_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_fake_bold_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_fake_bold_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_fake_bold_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_fake_bold_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_fake_bold_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_fake_bold_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_fake_bold_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_fake_bold_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_fake_bold_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var fakeBoldText = parms.fakeBoldText;
			var ret = CanvasKit._sk_paint_set_fake_bold_text(cpaint, fakeBoldText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_fake_bold_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_fake_bold_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_is_dev_kern_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_is_dev_kern_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_dev_kern_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_is_dev_kern_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_is_dev_kern_text(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_is_dev_kern_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_is_dev_kern_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_set_dev_kern_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_set_dev_kern_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_dev_kern_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_set_dev_kern_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var devKernText = parms.devKernText;
			var ret = CanvasKit._sk_paint_set_dev_kern_text(cpaint, devKernText);
			if((<any>SkiaSharp.ApiOverride).sk_paint_set_dev_kern_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_set_dev_kern_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_fill_path_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_paint_get_fill_path_0_Return();
			var parms = sk_paint_get_fill_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_0_Pre(parms);
			}
			var paint = parms.paint;
			var src = parms.src;
			var dst = parms.dst;
			var cullRect = parms.cullRect.marshalNew(CanvasKit);
			var resScale = parms.resScale;
			var ret = CanvasKit._sk_paint_get_fill_path(paint, src, dst, cullRect, resScale);
			var retStruct = new sk_paint_get_fill_path_0_Return();
			retStruct.cullRect = SkiaSharp.SKRect.unmarshal(cullRect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_paint_get_fill_path_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_fill_path_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_1_Pre(parms);
			}
			var paint = parms.paint;
			var src = parms.src;
			var dst = parms.dst;
			var cullRectZero = parms.cullRectZero;
			var resScale = parms.resScale;
			var ret = CanvasKit._sk_paint_get_fill_path(paint, src, dst, cullRectZero, resScale);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_fill_path_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_clone_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_clone_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_clone_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_clone_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var ret = CanvasKit._sk_paint_clone(cpaint);
			if((<any>SkiaSharp.ApiOverride).sk_paint_clone_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_clone_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_text_to_glyphs_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_text_to_glyphs_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_text_to_glyphs_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_text_to_glyphs_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var glyphs = parms.glyphs;
			var ret = CanvasKit._sk_paint_text_to_glyphs(cpaint, text, byteLength, glyphs);
			if((<any>SkiaSharp.ApiOverride).sk_paint_text_to_glyphs_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_text_to_glyphs_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_contains_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_contains_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_contains_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_contains_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var ret = CanvasKit._sk_paint_contains_text(cpaint, text, byteLength);
			if((<any>SkiaSharp.ApiOverride).sk_paint_contains_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_contains_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_count_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_count_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_count_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_count_text_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var ret = CanvasKit._sk_paint_count_text(cpaint, text, byteLength);
			if((<any>SkiaSharp.ApiOverride).sk_paint_count_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_count_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_widths_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_widths_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_widths_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_widths_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var widths = parms.widths;
			var bounds = parms.bounds;
			var ret = CanvasKit._sk_paint_get_text_widths(cpaint, text, byteLength, widths, bounds);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_widths_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_widths_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_text_intercepts_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_text_intercepts_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_intercepts_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_text_intercepts_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var x = parms.x;
			var y = parms.y;
			var bounds = CanvasKit._malloc(parms.bounds_Length * 4); /*float*/
			var bounds_f32 = bounds / 4;
			
			{
				for(var i = 0; i < parms.bounds_Length; i++)
				{
					CanvasKit.HEAPF32[bounds_f32 + i] = parms.bounds[i];
				}
			}
			var intervals = parms.intervals;
			var ret = CanvasKit._sk_paint_get_text_intercepts(cpaint, text, byteLength, x, y, bounds, intervals);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_text_intercepts_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_text_intercepts_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_pos_text_intercepts_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_pos_text_intercepts_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_intercepts_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_intercepts_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var pos = parms.pos;
			var bounds = CanvasKit._malloc(parms.bounds_Length * 4); /*float*/
			var bounds_f32 = bounds / 4;
			
			{
				for(var i = 0; i < parms.bounds_Length; i++)
				{
					CanvasKit.HEAPF32[bounds_f32 + i] = parms.bounds[i];
				}
			}
			var intervals = parms.intervals;
			var ret = CanvasKit._sk_paint_get_pos_text_intercepts(cpaint, text, byteLength, pos, bounds, intervals);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_intercepts_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_intercepts_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_pos_text_h_intercepts_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_pos_text_h_intercepts_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_h_intercepts_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_h_intercepts_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var text = parms.text;
			var byteLength = parms.byteLength;
			var xpos = CanvasKit._malloc(parms.xpos_Length * 4); /*float*/
			var xpos_f32 = xpos / 4;
			
			{
				for(var i = 0; i < parms.xpos_Length; i++)
				{
					CanvasKit.HEAPF32[xpos_f32 + i] = parms.xpos[i];
				}
			}
			var y = parms.y;
			var bounds = CanvasKit._malloc(parms.bounds_Length * 4); /*float*/
			var bounds_f32 = bounds / 4;
			
			{
				for(var i = 0; i < parms.bounds_Length; i++)
				{
					CanvasKit.HEAPF32[bounds_f32 + i] = parms.bounds[i];
				}
			}
			var intervals = parms.intervals;
			var ret = CanvasKit._sk_paint_get_pos_text_h_intercepts(cpaint, text, byteLength, xpos, y, bounds, intervals);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_h_intercepts_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_h_intercepts_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_paint_get_pos_text_blob_intercepts_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_paint_get_pos_text_blob_intercepts_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_blob_intercepts_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_blob_intercepts_0_Pre(parms);
			}
			var cpaint = parms.cpaint;
			var blob = parms.blob;
			var bounds = CanvasKit._malloc(parms.bounds_Length * 4); /*float*/
			var bounds_f32 = bounds / 4;
			
			{
				for(var i = 0; i < parms.bounds_Length; i++)
				{
					CanvasKit.HEAPF32[bounds_f32 + i] = parms.bounds[i];
				}
			}
			var intervals = parms.intervals;
			var ret = CanvasKit._sk_paint_get_pos_text_blob_intercepts(cpaint, blob, bounds, intervals);
			if((<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_blob_intercepts_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_paint_get_pos_text_blob_intercepts_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_ref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_ref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_ref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_ref_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_ref(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_ref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_ref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_unref_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_unref(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_raster_copy_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_new_raster_copy_0_Return();
			var parms = sk_image_new_raster_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_0_Pre(parms);
			}
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_image_new_raster_copy(info, pixels, rowBytes);
			var retStruct = new sk_image_new_raster_copy_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_new_raster_copy_with_pixmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_raster_copy_with_pixmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_with_pixmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_with_pixmap_0_Pre(parms);
			}
			var pixmap = parms.pixmap;
			var ret = CanvasKit._sk_image_new_raster_copy_with_pixmap(pixmap);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_with_pixmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_raster_copy_with_pixmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_raster_data_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_new_raster_data_0_Return();
			var parms = sk_image_new_raster_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_raster_data_0_Pre(parms);
			}
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_image_new_raster_data(info, pixels, rowBytes);
			var retStruct = new sk_image_new_raster_data_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_raster_data_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_new_raster_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_raster_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_raster_0_Pre(parms);
			}
			var pixmap = parms.pixmap;
			var releaseProc = parms.releaseProc;
			var context = parms.context;
			var ret = CanvasKit._sk_image_new_raster(pixmap, releaseProc, context);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_raster_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_raster_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_from_bitmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_from_bitmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_bitmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_bitmap_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var ret = CanvasKit._sk_image_new_from_bitmap(cbitmap);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_bitmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_bitmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_from_encoded_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_new_from_encoded_0_Return();
			var parms = sk_image_new_from_encoded_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_0_Pre(parms);
			}
			var encoded = parms.encoded;
			var subset = parms.subset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_image_new_from_encoded(encoded, subset);
			var retStruct = new sk_image_new_from_encoded_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_new_from_encoded_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_from_encoded_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_1_Pre(parms);
			}
			var encoded = parms.encoded;
			var subsetZero = parms.subsetZero;
			var ret = CanvasKit._sk_image_new_from_encoded(encoded, subsetZero);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_encoded_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_from_texture_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_from_texture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_texture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_texture_0_Pre(parms);
			}
			var context = parms.context;
			var texture = parms.texture;
			var origin = parms.origin;
			var colorType = parms.colorType;
			var alpha = parms.alpha;
			var colorSpace = parms.colorSpace;
			var releaseProc = parms.releaseProc;
			var releaseContext = parms.releaseContext;
			var ret = CanvasKit._sk_image_new_from_texture(context, texture, origin, colorType, alpha, colorSpace, releaseProc, releaseContext);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_texture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_texture_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_from_adopted_texture_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_new_from_adopted_texture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_adopted_texture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_adopted_texture_0_Pre(parms);
			}
			var context = parms.context;
			var texture = parms.texture;
			var origin = parms.origin;
			var colorType = parms.colorType;
			var alpha = parms.alpha;
			var colorSpace = parms.colorSpace;
			var ret = CanvasKit._sk_image_new_from_adopted_texture(context, texture, origin, colorType, alpha, colorSpace);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_adopted_texture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_adopted_texture_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_new_from_picture_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_new_from_picture_0_Return();
			var parms = sk_image_new_from_picture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_0_Pre(parms);
			}
			var picture = parms.picture;
			var dimensions = parms.dimensions.marshalNew(CanvasKit);
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var paint = parms.paint;
			var ret = CanvasKit._sk_image_new_from_picture(picture, dimensions, matrix, paint);
			var retStruct = new sk_image_new_from_picture_0_Return();
			retStruct.dimensions = SkiaSharp.SKSizeI.unmarshal(dimensions, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_new_from_picture_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_new_from_picture_1_Return();
			var parms = sk_image_new_from_picture_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_1_Pre(parms);
			}
			var picture = parms.picture;
			var dimensions = parms.dimensions.marshalNew(CanvasKit);
			var matrixZero = parms.matrixZero;
			var paint = parms.paint;
			var ret = CanvasKit._sk_image_new_from_picture(picture, dimensions, matrixZero, paint);
			var retStruct = new sk_image_new_from_picture_1_Return();
			retStruct.dimensions = SkiaSharp.SKSizeI.unmarshal(dimensions, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_new_from_picture_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_get_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_width_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_width(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_get_height_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_height_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_height_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_height_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_height(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_height_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_height_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_get_unique_id_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_unique_id_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_unique_id_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_unique_id_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_unique_id(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_unique_id_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_unique_id_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_get_alpha_type_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_alpha_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_alpha_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_alpha_type_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_alpha_type(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_alpha_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_alpha_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_get_color_type_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_color_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_color_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_color_type_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_color_type(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_color_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_color_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_get_colorspace_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_get_colorspace_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_colorspace_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_get_colorspace_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_get_colorspace(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_get_colorspace_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_get_colorspace_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_is_alpha_only_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_is_alpha_only_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_alpha_only_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_is_alpha_only_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_is_alpha_only(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_alpha_only_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_is_alpha_only_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_make_shader_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_make_shader_0_Return();
			var parms = sk_image_make_shader_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_shader_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_make_shader_0_Pre(parms);
			}
			var image = parms.image;
			var tileX = parms.tileX;
			var tileY = parms.tileY;
			var localMatrix = parms.localMatrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_image_make_shader(image, tileX, tileY, localMatrix);
			var retStruct = new sk_image_make_shader_0_Return();
			retStruct.localMatrix = SkiaSharp.SKMatrix.unmarshal(localMatrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_shader_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_make_shader_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_make_shader_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_make_shader_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_shader_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_make_shader_1_Pre(parms);
			}
			var image = parms.image;
			var tileX = parms.tileX;
			var tileY = parms.tileY;
			var localMatrixZero = parms.localMatrixZero;
			var ret = CanvasKit._sk_image_make_shader(image, tileX, tileY, localMatrixZero);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_shader_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_make_shader_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_peek_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_peek_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_peek_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_peek_pixels_0_Pre(parms);
			}
			var image = parms.image;
			var pixmap = parms.pixmap;
			var ret = CanvasKit._sk_image_peek_pixels(image, pixmap);
			if((<any>SkiaSharp.ApiOverride).sk_image_peek_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_peek_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_is_texture_backed_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_is_texture_backed_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_texture_backed_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_is_texture_backed_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_is_texture_backed(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_texture_backed_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_is_texture_backed_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_is_lazy_generated_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_is_lazy_generated_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_lazy_generated_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_is_lazy_generated_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_is_lazy_generated(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_is_lazy_generated_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_is_lazy_generated_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_read_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_read_pixels_0_Return();
			var parms = sk_image_read_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_read_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_read_pixels_0_Pre(parms);
			}
			var image = parms.image;
			var dstInfo = parms.dstInfo.marshalNew(CanvasKit);
			var dstPixels = parms.dstPixels;
			var dstRowBytes = parms.dstRowBytes;
			var srcX = parms.srcX;
			var srcY = parms.srcY;
			var cachingHint = parms.cachingHint;
			var ret = CanvasKit._sk_image_read_pixels(image, dstInfo, dstPixels, dstRowBytes, srcX, srcY, cachingHint);
			var retStruct = new sk_image_read_pixels_0_Return();
			retStruct.dstInfo = SkiaSharp.SKImageInfoNative.unmarshal(dstInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_read_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_read_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_read_pixels_into_pixmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_read_pixels_into_pixmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_read_pixels_into_pixmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_read_pixels_into_pixmap_0_Pre(parms);
			}
			var image = parms.image;
			var dst = parms.dst;
			var srcX = parms.srcX;
			var srcY = parms.srcY;
			var cachingHint = parms.cachingHint;
			var ret = CanvasKit._sk_image_read_pixels_into_pixmap(image, dst, srcX, srcY, cachingHint);
			if((<any>SkiaSharp.ApiOverride).sk_image_read_pixels_into_pixmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_read_pixels_into_pixmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_scale_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_scale_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_scale_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_scale_pixels_0_Pre(parms);
			}
			var image = parms.image;
			var dst = parms.dst;
			var quality = parms.quality;
			var cachingHint = parms.cachingHint;
			var ret = CanvasKit._sk_image_scale_pixels(image, dst, quality, cachingHint);
			if((<any>SkiaSharp.ApiOverride).sk_image_scale_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_scale_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_ref_encoded_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_ref_encoded_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_ref_encoded_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_ref_encoded_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_ref_encoded(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_ref_encoded_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_ref_encoded_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_encode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_encode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_encode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_encode_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_encode(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_encode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_encode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_encode_specific_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_encode_specific_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_encode_specific_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_encode_specific_0_Pre(parms);
			}
			var image = parms.image;
			var encoder = parms.encoder;
			var quality = parms.quality;
			var ret = CanvasKit._sk_image_encode_specific(image, encoder, quality);
			if((<any>SkiaSharp.ApiOverride).sk_image_encode_specific_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_encode_specific_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_make_subset_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_make_subset_0_Return();
			var parms = sk_image_make_subset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_subset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_make_subset_0_Pre(parms);
			}
			var image = parms.image;
			var subset = parms.subset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_image_make_subset(image, subset);
			var retStruct = new sk_image_make_subset_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_subset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_make_subset_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_image_make_non_texture_image_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_image_make_non_texture_image_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_non_texture_image_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_make_non_texture_image_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_image_make_non_texture_image(image);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_non_texture_image_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_make_non_texture_image_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_image_make_with_filter_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_image_make_with_filter_0_Return();
			var parms = sk_image_make_with_filter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_with_filter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_image_make_with_filter_0_Pre(parms);
			}
			var image = parms.image;
			var filter = parms.filter;
			var subset = parms.subset.marshalNew(CanvasKit);
			var clipbounds = parms.clipbounds.marshalNew(CanvasKit);
			var outSubset = retStruct.outSubset.marshalNew(CanvasKit);
			var outOffset = retStruct.outOffset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_image_make_with_filter(image, filter, subset, clipbounds, outSubset, outOffset);
			var retStruct = new sk_image_make_with_filter_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			retStruct.clipbounds = SkiaSharp.SKRectI.unmarshal(clipbounds, CanvasKit);
			retStruct.outSubset = SkiaSharp.SKRectI.unmarshal(outSubset, CanvasKit);
			retStruct.outOffset = SkiaSharp.SKPoint.unmarshal(outOffset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_image_make_with_filter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_image_make_with_filter_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_contains_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_contains_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_contains_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_contains_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_path_contains(cpath, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_path_contains_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_contains_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_get_last_point_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_get_last_point_0_Return();
			var parms = sk_path_get_last_point_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_last_point_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_last_point_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var point = retStruct.point.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_get_last_point(cpath, point);
			var retStruct = new sk_path_get_last_point_0_Return();
			retStruct.point = SkiaSharp.SKPoint.unmarshal(point, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_last_point_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_last_point_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_path_new();
			if((<any>SkiaSharp.ApiOverride).sk_path_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_path_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_delete_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_delete(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_move_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_move_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_move_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_move_to_0_Pre(parms);
			}
			var t = parms.t;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_path_move_to(t, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_path_move_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_move_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rmove_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rmove_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rmove_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rmove_to_0_Pre(parms);
			}
			var t = parms.t;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_path_rmove_to(t, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_path_rmove_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rmove_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_line_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_line_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_line_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_line_to_0_Pre(parms);
			}
			var t = parms.t;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_path_line_to(t, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_path_line_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_line_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rline_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rline_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rline_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rline_to_0_Pre(parms);
			}
			var t = parms.t;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_path_rline_to(t, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_path_rline_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rline_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_quad_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_quad_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_quad_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_quad_to_0_Pre(parms);
			}
			var t = parms.t;
			var x0 = parms.x0;
			var y0 = parms.y0;
			var x1 = parms.x1;
			var y1 = parms.y1;
			var ret = CanvasKit._sk_path_quad_to(t, x0, y0, x1, y1);
			if((<any>SkiaSharp.ApiOverride).sk_path_quad_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_quad_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rquad_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rquad_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rquad_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rquad_to_0_Pre(parms);
			}
			var t = parms.t;
			var dx0 = parms.dx0;
			var dy0 = parms.dy0;
			var dx1 = parms.dx1;
			var dy1 = parms.dy1;
			var ret = CanvasKit._sk_path_rquad_to(t, dx0, dy0, dx1, dy1);
			if((<any>SkiaSharp.ApiOverride).sk_path_rquad_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rquad_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_conic_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_conic_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_conic_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_conic_to_0_Pre(parms);
			}
			var t = parms.t;
			var x0 = parms.x0;
			var y0 = parms.y0;
			var x1 = parms.x1;
			var y1 = parms.y1;
			var w = parms.w;
			var ret = CanvasKit._sk_path_conic_to(t, x0, y0, x1, y1, w);
			if((<any>SkiaSharp.ApiOverride).sk_path_conic_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_conic_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rconic_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rconic_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rconic_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rconic_to_0_Pre(parms);
			}
			var t = parms.t;
			var dx0 = parms.dx0;
			var dy0 = parms.dy0;
			var dx1 = parms.dx1;
			var dy1 = parms.dy1;
			var w = parms.w;
			var ret = CanvasKit._sk_path_rconic_to(t, dx0, dy0, dx1, dy1, w);
			if((<any>SkiaSharp.ApiOverride).sk_path_rconic_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rconic_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_cubic_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_cubic_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_cubic_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_cubic_to_0_Pre(parms);
			}
			var t = parms.t;
			var x0 = parms.x0;
			var y0 = parms.y0;
			var x1 = parms.x1;
			var y1 = parms.y1;
			var x2 = parms.x2;
			var y2 = parms.y2;
			var ret = CanvasKit._sk_path_cubic_to(t, x0, y0, x1, y1, x2, y2);
			if((<any>SkiaSharp.ApiOverride).sk_path_cubic_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_cubic_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rcubic_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rcubic_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rcubic_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rcubic_to_0_Pre(parms);
			}
			var t = parms.t;
			var dx0 = parms.dx0;
			var dy0 = parms.dy0;
			var dx1 = parms.dx1;
			var dy1 = parms.dy1;
			var dx2 = parms.dx2;
			var dy2 = parms.dy2;
			var ret = CanvasKit._sk_path_rcubic_to(t, dx0, dy0, dx1, dy1, dx2, dy2);
			if((<any>SkiaSharp.ApiOverride).sk_path_rcubic_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rcubic_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_close_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_close_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_close_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_close_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_close(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_close_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_close_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rewind_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rewind_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rewind_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rewind_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_rewind(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_rewind_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rewind_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_reset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_reset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_reset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_reset_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_reset(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_reset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_reset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_rect_0_Return();
			var parms = sk_path_add_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_rect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var direction = parms.direction;
			var ret = CanvasKit._sk_path_add_rect(t, rect, direction);
			var retStruct = new sk_path_add_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_rect_start_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_rect_start_0_Return();
			var parms = sk_path_add_rect_start_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rect_start_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_rect_start_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var direction = parms.direction;
			var startIndex = parms.startIndex;
			var ret = CanvasKit._sk_path_add_rect_start(t, rect, direction, startIndex);
			var retStruct = new sk_path_add_rect_start_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rect_start_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_rect_start_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_rrect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_rrect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rrect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_rrect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect;
			var direction = parms.direction;
			var ret = CanvasKit._sk_path_add_rrect(t, rect, direction);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rrect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_rrect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_rrect_start_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_rrect_start_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rrect_start_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_rrect_start_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect;
			var direction = parms.direction;
			var startIndex = parms.startIndex;
			var ret = CanvasKit._sk_path_add_rrect_start(t, rect, direction, startIndex);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rrect_start_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_rrect_start_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_oval_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_oval_0_Return();
			var parms = sk_path_add_oval_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_oval_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_oval_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var direction = parms.direction;
			var ret = CanvasKit._sk_path_add_oval(t, rect, direction);
			var retStruct = new sk_path_add_oval_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_oval_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_oval_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_arc_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_arc_0_Return();
			var parms = sk_path_add_arc_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_arc_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_arc_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var startAngle = parms.startAngle;
			var sweepAngle = parms.sweepAngle;
			var ret = CanvasKit._sk_path_add_arc(t, rect, startAngle, sweepAngle);
			var retStruct = new sk_path_add_arc_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_arc_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_arc_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_path_offset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_path_offset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_offset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_path_offset_0_Pre(parms);
			}
			var t = parms.t;
			var other = parms.other;
			var dx = parms.dx;
			var dy = parms.dy;
			var mode = parms.mode;
			var ret = CanvasKit._sk_path_add_path_offset(t, other, dx, dy, mode);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_offset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_path_offset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_path_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_path_matrix_0_Return();
			var parms = sk_path_add_path_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_path_matrix_0_Pre(parms);
			}
			var t = parms.t;
			var other = parms.other;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var mode = parms.mode;
			var ret = CanvasKit._sk_path_add_path_matrix(t, other, matrix, mode);
			var retStruct = new sk_path_add_path_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_path_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_path_0_Pre(parms);
			}
			var t = parms.t;
			var other = parms.other;
			var mode = parms.mode;
			var ret = CanvasKit._sk_path_add_path(t, other, mode);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_path_reverse_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_path_reverse_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_reverse_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_path_reverse_0_Pre(parms);
			}
			var t = parms.t;
			var other = parms.other;
			var ret = CanvasKit._sk_path_add_path_reverse(t, other);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_path_reverse_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_path_reverse_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_get_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_get_bounds_0_Return();
			var parms = sk_path_get_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_bounds_0_Pre(parms);
			}
			var t = parms.t;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_get_bounds(t, rect);
			var retStruct = new sk_path_get_bounds_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_compute_tight_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_compute_tight_bounds_0_Return();
			var parms = sk_path_compute_tight_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_compute_tight_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_compute_tight_bounds_0_Pre(parms);
			}
			var t = parms.t;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_compute_tight_bounds(t, rect);
			var retStruct = new sk_path_compute_tight_bounds_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_compute_tight_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_compute_tight_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_get_filltype_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_get_filltype_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_filltype_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_filltype_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_get_filltype(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_filltype_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_filltype_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_set_filltype_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_set_filltype_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_set_filltype_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_set_filltype_0_Pre(parms);
			}
			var t = parms.t;
			var filltype = parms.filltype;
			var ret = CanvasKit._sk_path_set_filltype(t, filltype);
			if((<any>SkiaSharp.ApiOverride).sk_path_set_filltype_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_set_filltype_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_clone_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_clone_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_clone_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_clone_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_clone(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_clone_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_clone_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_transform_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_transform_0_Return();
			var parms = sk_path_transform_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_transform_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_transform_0_Pre(parms);
			}
			var t = parms.t;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_transform(t, matrix);
			var retStruct = new sk_path_transform_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_transform_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_transform_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_arc_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_arc_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_arc_to_0_Pre(parms);
			}
			var t = parms.t;
			var rx = parms.rx;
			var ry = parms.ry;
			var xAxisRotate = parms.xAxisRotate;
			var largeArc = parms.largeArc;
			var sweep = parms.sweep;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_path_arc_to(t, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_arc_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rarc_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rarc_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rarc_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rarc_to_0_Pre(parms);
			}
			var t = parms.t;
			var rx = parms.rx;
			var ry = parms.ry;
			var xAxisRotate = parms.xAxisRotate;
			var largeArc = parms.largeArc;
			var sweep = parms.sweep;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_path_rarc_to(t, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_path_rarc_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rarc_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_arc_to_with_oval_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_arc_to_with_oval_0_Return();
			var parms = sk_path_arc_to_with_oval_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_oval_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_oval_0_Pre(parms);
			}
			var t = parms.t;
			var oval = parms.oval.marshalNew(CanvasKit);
			var startAngle = parms.startAngle;
			var sweepAngle = parms.sweepAngle;
			var forceMoveTo = parms.forceMoveTo;
			var ret = CanvasKit._sk_path_arc_to_with_oval(t, oval, startAngle, sweepAngle, forceMoveTo);
			var retStruct = new sk_path_arc_to_with_oval_0_Return();
			retStruct.oval = SkiaSharp.SKRect.unmarshal(oval, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_oval_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_oval_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_arc_to_with_points_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_arc_to_with_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_points_0_Pre(parms);
			}
			var t = parms.t;
			var x1 = parms.x1;
			var y1 = parms.y1;
			var x2 = parms.x2;
			var y2 = parms.y2;
			var radius = parms.radius;
			var ret = CanvasKit._sk_path_arc_to_with_points(t, x1, y1, x2, y2, radius);
			if((<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_arc_to_with_points_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_add_rounded_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_add_rounded_rect_0_Return();
			var parms = sk_path_add_rounded_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rounded_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_rounded_rect_0_Pre(parms);
			}
			var t = parms.t;
			var rect = parms.rect.marshalNew(CanvasKit);
			var rx = parms.rx;
			var ry = parms.ry;
			var dir = parms.dir;
			var ret = CanvasKit._sk_path_add_rounded_rect(t, rect, rx, ry, dir);
			var retStruct = new sk_path_add_rounded_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_rounded_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_rounded_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_circle_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_circle_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_circle_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_circle_0_Pre(parms);
			}
			var t = parms.t;
			var x = parms.x;
			var y = parms.y;
			var radius = parms.radius;
			var dir = parms.dir;
			var ret = CanvasKit._sk_path_add_circle(t, x, y, radius, dir);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_circle_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_circle_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_count_verbs_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_count_verbs_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_count_verbs_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_count_verbs_0_Pre(parms);
			}
			var path = parms.path;
			var ret = CanvasKit._sk_path_count_verbs(path);
			if((<any>SkiaSharp.ApiOverride).sk_path_count_verbs_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_count_verbs_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_count_points_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_count_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_count_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_count_points_0_Pre(parms);
			}
			var path = parms.path;
			var ret = CanvasKit._sk_path_count_points(path);
			if((<any>SkiaSharp.ApiOverride).sk_path_count_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_count_points_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_get_point_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_get_point_0_Return();
			var parms = sk_path_get_point_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_point_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_point_0_Pre(parms);
			}
			var path = parms.path;
			var index = parms.index;
			var point = retStruct.point.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_get_point(path, index, point);
			var retStruct = new sk_path_get_point_0_Return();
			retStruct.point = SkiaSharp.SKPoint.unmarshal(point, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_point_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_point_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_get_points_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_get_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_points_0_Pre(parms);
			}
			var path = parms.path;
			var points = parms.points;
			var max = parms.max;
			var ret = CanvasKit._sk_path_get_points(path, points, max);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_points_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_get_convexity_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_get_convexity_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_convexity_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_convexity_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var ret = CanvasKit._sk_path_get_convexity(cpath);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_convexity_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_convexity_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_set_convexity_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_set_convexity_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_set_convexity_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_set_convexity_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var convexity = parms.convexity;
			var ret = CanvasKit._sk_path_set_convexity(cpath, convexity);
			if((<any>SkiaSharp.ApiOverride).sk_path_set_convexity_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_set_convexity_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_parse_svg_string_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_parse_svg_string_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_parse_svg_string_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_parse_svg_string_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var str_Length = parms.str.length*4+1
			var str = CanvasKit._malloc(str_Length);
			CanvasKit.stringToUTF8(parms.str, str, str_Length);
			var ret = CanvasKit._sk_path_parse_svg_string(cpath, str);
			if((<any>SkiaSharp.ApiOverride).sk_path_parse_svg_string_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_parse_svg_string_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_to_svg_string_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_to_svg_string_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_to_svg_string_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_to_svg_string_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var str = parms.str;
			var ret = CanvasKit._sk_path_to_svg_string(cpath, str);
			if((<any>SkiaSharp.ApiOverride).sk_path_to_svg_string_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_to_svg_string_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_convert_conic_to_quads_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_convert_conic_to_quads_0_Return();
			var parms = sk_path_convert_conic_to_quads_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_convert_conic_to_quads_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_convert_conic_to_quads_0_Pre(parms);
			}
			var p0 = parms.p0.marshalNew(CanvasKit);
			var p1 = parms.p1.marshalNew(CanvasKit);
			var p2 = parms.p2.marshalNew(CanvasKit);
			var w = parms.w;
			var pts = parms.pts;
			var pow2 = parms.pow2;
			var ret = CanvasKit._sk_path_convert_conic_to_quads(p0, p1, p2, w, pts, pow2);
			var retStruct = new sk_path_convert_conic_to_quads_0_Return();
			retStruct.p0 = SkiaSharp.SKPoint.unmarshal(p0, CanvasKit);
			retStruct.p1 = SkiaSharp.SKPoint.unmarshal(p1, CanvasKit);
			retStruct.p2 = SkiaSharp.SKPoint.unmarshal(p2, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_convert_conic_to_quads_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_convert_conic_to_quads_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_add_poly_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_add_poly_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_poly_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_add_poly_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var points = parms.points;
			var count = parms.count;
			var close = parms.close;
			var ret = CanvasKit._sk_path_add_poly(cpath, points, count, close);
			if((<any>SkiaSharp.ApiOverride).sk_path_add_poly_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_add_poly_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_get_segment_masks_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_get_segment_masks_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_segment_masks_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_get_segment_masks_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_path_get_segment_masks(t);
			if((<any>SkiaSharp.ApiOverride).sk_path_get_segment_masks_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_get_segment_masks_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_is_oval_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_is_oval_0_Return();
			var parms = sk_path_is_oval_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_oval_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_oval_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var bounds = retStruct.bounds.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_is_oval(cpath, bounds);
			var retStruct = new sk_path_is_oval_0_Return();
			retStruct.bounds = SkiaSharp.SKRect.unmarshal(bounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_oval_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_oval_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_is_oval_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_is_oval_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_oval_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_oval_1_Pre(parms);
			}
			var cpath = parms.cpath;
			var boundsZero = parms.boundsZero;
			var ret = CanvasKit._sk_path_is_oval(cpath, boundsZero);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_oval_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_oval_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_is_rrect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_is_rrect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rrect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_rrect_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var bounds = parms.bounds;
			var ret = CanvasKit._sk_path_is_rrect(cpath, bounds);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rrect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_rrect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_is_line_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_is_line_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_line_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_line_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var line = parms.line;
			var ret = CanvasKit._sk_path_is_line(cpath, line);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_line_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_line_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_is_line_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_is_line_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_line_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_line_1_Pre(parms);
			}
			var cpath = parms.cpath;
			var lineZero = parms.lineZero;
			var ret = CanvasKit._sk_path_is_line(cpath, lineZero);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_line_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_line_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_is_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_is_rect_0_Return();
			var parms = sk_path_is_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_rect_0_Pre(parms);
			}
			var cpath = parms.cpath;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var isClosed = CanvasKit._malloc(4);
			var direction = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_path_is_rect(cpath, rect, isClosed, direction);
			var retStruct = new sk_path_is_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			retStruct.isClosed = CanvasKit.getValue(isClosed, "i32");
			retStruct.direction = CanvasKit.getValue(direction, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_is_rect_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_is_rect_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rect_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_is_rect_1_Pre(parms);
			}
			var cpath = parms.cpath;
			var rectZero = parms.rectZero;
			var isClosedZero = parms.isClosedZero;
			var directionZero = parms.directionZero;
			var ret = CanvasKit._sk_path_is_rect(cpath, rectZero, isClosedZero, directionZero);
			if((<any>SkiaSharp.ApiOverride).sk_path_is_rect_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_is_rect_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_pathmeasure_new();
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_pathmeasure_new_with_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_new_with_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_with_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_with_path_0_Pre(parms);
			}
			var path = parms.path;
			var forceClosed = parms.forceClosed;
			var resScale = parms.resScale;
			var ret = CanvasKit._sk_pathmeasure_new_with_path(path, forceClosed, resScale);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_with_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_new_with_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_destroy_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var ret = CanvasKit._sk_pathmeasure_destroy(pathMeasure);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_set_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_set_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_set_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_set_path_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var path = parms.path;
			var forceClosed = parms.forceClosed;
			var ret = CanvasKit._sk_pathmeasure_set_path(pathMeasure, path, forceClosed);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_set_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_set_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_get_length_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_get_length_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_length_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_length_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var ret = CanvasKit._sk_pathmeasure_get_length(pathMeasure);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_length_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_length_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_get_pos_tan_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pathmeasure_get_pos_tan_0_Return();
			var parms = sk_pathmeasure_get_pos_tan_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var distance = parms.distance;
			var position = retStruct.position.marshalNew(CanvasKit);
			var tangent = retStruct.tangent.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pathmeasure_get_pos_tan(pathMeasure, distance, position, tangent);
			var retStruct = new sk_pathmeasure_get_pos_tan_0_Return();
			retStruct.position = SkiaSharp.SKPoint.unmarshal(position, CanvasKit);
			retStruct.tangent = SkiaSharp.SKPoint.unmarshal(tangent, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pathmeasure_get_pos_tan_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pathmeasure_get_pos_tan_1_Return();
			var parms = sk_pathmeasure_get_pos_tan_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_1_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var distance = parms.distance;
			var positionZero = parms.positionZero;
			var tangent = retStruct.tangent.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pathmeasure_get_pos_tan(pathMeasure, distance, positionZero, tangent);
			var retStruct = new sk_pathmeasure_get_pos_tan_1_Return();
			retStruct.tangent = SkiaSharp.SKPoint.unmarshal(tangent, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pathmeasure_get_pos_tan_2(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pathmeasure_get_pos_tan_2_Return();
			var parms = sk_pathmeasure_get_pos_tan_2_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_2_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_2_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var distance = parms.distance;
			var position = retStruct.position.marshalNew(CanvasKit);
			var tangentZero = parms.tangentZero;
			var ret = CanvasKit._sk_pathmeasure_get_pos_tan(pathMeasure, distance, position, tangentZero);
			var retStruct = new sk_pathmeasure_get_pos_tan_2_Return();
			retStruct.position = SkiaSharp.SKPoint.unmarshal(position, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_2_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_pos_tan_2_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pathmeasure_get_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pathmeasure_get_matrix_0_Return();
			var parms = sk_pathmeasure_get_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_matrix_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var distance = parms.distance;
			var matrix = retStruct.matrix.marshalNew(CanvasKit);
			var flags = parms.flags;
			var ret = CanvasKit._sk_pathmeasure_get_matrix(pathMeasure, distance, matrix, flags);
			var retStruct = new sk_pathmeasure_get_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pathmeasure_get_segment_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_get_segment_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_segment_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_segment_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var start = parms.start;
			var stop = parms.stop;
			var dst = parms.dst;
			var startWithMoveTo = parms.startWithMoveTo;
			var ret = CanvasKit._sk_pathmeasure_get_segment(pathMeasure, start, stop, dst, startWithMoveTo);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_segment_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_get_segment_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_is_closed_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_is_closed_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_is_closed_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_is_closed_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var ret = CanvasKit._sk_pathmeasure_is_closed(pathMeasure);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_is_closed_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_is_closed_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathmeasure_next_contour_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathmeasure_next_contour_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_next_contour_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathmeasure_next_contour_0_Pre(parms);
			}
			var pathMeasure = parms.pathMeasure;
			var ret = CanvasKit._sk_pathmeasure_next_contour(pathMeasure);
			if((<any>SkiaSharp.ApiOverride).sk_pathmeasure_next_contour_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathmeasure_next_contour_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathop_op_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathop_op_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_op_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathop_op_0_Pre(parms);
			}
			var one = parms.one;
			var two = parms.two;
			var op = parms.op;
			var result = parms.result;
			var ret = CanvasKit._sk_pathop_op(one, two, op, result);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_op_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathop_op_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathop_simplify_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pathop_simplify_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_simplify_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathop_simplify_0_Pre(parms);
			}
			var path = parms.path;
			var result = parms.result;
			var ret = CanvasKit._sk_pathop_simplify(path, result);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_simplify_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathop_simplify_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pathop_tight_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pathop_tight_bounds_0_Return();
			var parms = sk_pathop_tight_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_tight_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pathop_tight_bounds_0_Pre(parms);
			}
			var path = parms.path;
			var result = retStruct.result.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pathop_tight_bounds(path, result);
			var retStruct = new sk_pathop_tight_bounds_0_Return();
			retStruct.result = SkiaSharp.SKRect.unmarshal(result, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pathop_tight_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pathop_tight_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_opbuilder_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_opbuilder_new();
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_opbuilder_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_opbuilder_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_opbuilder_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_opbuilder_destroy_0_Pre(parms);
			}
			var builder = parms.builder;
			var ret = CanvasKit._sk_opbuilder_destroy(builder);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_opbuilder_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_opbuilder_add_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_opbuilder_add_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_add_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_opbuilder_add_0_Pre(parms);
			}
			var builder = parms.builder;
			var path = parms.path;
			var op = parms.op;
			var ret = CanvasKit._sk_opbuilder_add(builder, path, op);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_add_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_opbuilder_add_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_opbuilder_resolve_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_opbuilder_resolve_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_resolve_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_opbuilder_resolve_0_Pre(parms);
			}
			var builder = parms.builder;
			var result = parms.result;
			var ret = CanvasKit._sk_opbuilder_resolve(builder, result);
			if((<any>SkiaSharp.ApiOverride).sk_opbuilder_resolve_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_opbuilder_resolve_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_create_iter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_create_iter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_create_iter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_create_iter_0_Pre(parms);
			}
			var path = parms.path;
			var forceClose = parms.forceClose;
			var ret = CanvasKit._sk_path_create_iter(path, forceClose);
			if((<any>SkiaSharp.ApiOverride).sk_path_create_iter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_create_iter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_iter_next_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_iter_next_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_next_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_iter_next_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var points = parms.points;
			var doConsumeDegenerates = parms.doConsumeDegenerates;
			var exact = parms.exact;
			var ret = CanvasKit._sk_path_iter_next(iterator, points, doConsumeDegenerates, exact);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_next_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_iter_next_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_iter_conic_weight_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_iter_conic_weight_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_conic_weight_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_iter_conic_weight_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var ret = CanvasKit._sk_path_iter_conic_weight(iterator);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_conic_weight_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_iter_conic_weight_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_iter_is_close_line_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_iter_is_close_line_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_is_close_line_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_iter_is_close_line_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var ret = CanvasKit._sk_path_iter_is_close_line(iterator);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_is_close_line_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_iter_is_close_line_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_iter_is_closed_contour_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_iter_is_closed_contour_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_is_closed_contour_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_iter_is_closed_contour_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var ret = CanvasKit._sk_path_iter_is_closed_contour(iterator);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_is_closed_contour_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_iter_is_closed_contour_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_iter_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_iter_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_iter_destroy_0_Pre(parms);
			}
			var path = parms.path;
			var ret = CanvasKit._sk_path_iter_destroy(path);
			if((<any>SkiaSharp.ApiOverride).sk_path_iter_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_iter_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_create_rawiter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_create_rawiter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_create_rawiter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_create_rawiter_0_Pre(parms);
			}
			var path = parms.path;
			var ret = CanvasKit._sk_path_create_rawiter(path);
			if((<any>SkiaSharp.ApiOverride).sk_path_create_rawiter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_create_rawiter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rawiter_next_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rawiter_next_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_next_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rawiter_next_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var points = parms.points;
			var ret = CanvasKit._sk_path_rawiter_next(iterator, points);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_next_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rawiter_next_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rawiter_peek_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rawiter_peek_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_peek_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rawiter_peek_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var ret = CanvasKit._sk_path_rawiter_peek(iterator);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_peek_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rawiter_peek_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rawiter_conic_weight_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rawiter_conic_weight_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_conic_weight_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rawiter_conic_weight_0_Pre(parms);
			}
			var iterator = parms.iterator;
			var ret = CanvasKit._sk_path_rawiter_conic_weight(iterator);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_conic_weight_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rawiter_conic_weight_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_rawiter_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_rawiter_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_rawiter_destroy_0_Pre(parms);
			}
			var path = parms.path;
			var ret = CanvasKit._sk_path_rawiter_destroy(path);
			if((<any>SkiaSharp.ApiOverride).sk_path_rawiter_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_rawiter_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_maskfilter_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_maskfilter_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_unref_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_maskfilter_unref(t);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_maskfilter_new_blur_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_maskfilter_new_blur_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_0_Pre(parms);
			}
			var style = parms.style;
			var sigma = parms.sigma;
			var ret = CanvasKit._sk_maskfilter_new_blur(style, sigma);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_maskfilter_new_blur_with_flags_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_maskfilter_new_blur_with_flags_0_Return();
			var parms = sk_maskfilter_new_blur_with_flags_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_with_flags_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_with_flags_0_Pre(parms);
			}
			var style = parms.style;
			var sigma = parms.sigma;
			var occluder = parms.occluder.marshalNew(CanvasKit);
			var respectCTM = parms.respectCTM;
			var ret = CanvasKit._sk_maskfilter_new_blur_with_flags(style, sigma, occluder, respectCTM);
			var retStruct = new sk_maskfilter_new_blur_with_flags_0_Return();
			retStruct.occluder = SkiaSharp.SKRect.unmarshal(occluder, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_with_flags_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_new_blur_with_flags_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_maskfilter_new_table_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_maskfilter_new_table_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_table_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_new_table_0_Pre(parms);
			}
			var table = CanvasKit._malloc(parms.table_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.table_Length; i++)
				{
					CanvasKit.HEAPU8[table + i] = parms.table[i];
				}
			}
			var ret = CanvasKit._sk_maskfilter_new_table(table);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_table_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_new_table_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_maskfilter_new_gamma_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_maskfilter_new_gamma_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_gamma_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_new_gamma_0_Pre(parms);
			}
			var gamma = parms.gamma;
			var ret = CanvasKit._sk_maskfilter_new_gamma(gamma);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_gamma_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_new_gamma_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_maskfilter_new_clip_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_maskfilter_new_clip_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_clip_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_maskfilter_new_clip_0_Pre(parms);
			}
			var min = parms.min;
			var max = parms.max;
			var ret = CanvasKit._sk_maskfilter_new_clip(min, max);
			if((<any>SkiaSharp.ApiOverride).sk_maskfilter_new_clip_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_maskfilter_new_clip_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_croprect_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_imagefilter_croprect_new();
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_imagefilter_croprect_new_with_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_croprect_new_with_rect_0_Return();
			var parms = sk_imagefilter_croprect_new_with_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_with_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_with_rect_0_Pre(parms);
			}
			var rect = parms.rect.marshalNew(CanvasKit);
			var flags = parms.flags;
			var ret = CanvasKit._sk_imagefilter_croprect_new_with_rect(rect, flags);
			var retStruct = new sk_imagefilter_croprect_new_with_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_with_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_new_with_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_croprect_destructor_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_croprect_destructor_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_destructor_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_destructor_0_Pre(parms);
			}
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_croprect_destructor(cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_destructor_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_destructor_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_croprect_get_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_croprect_get_rect_0_Return();
			var parms = sk_imagefilter_croprect_get_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_rect_0_Pre(parms);
			}
			var cropRect = parms.cropRect;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_imagefilter_croprect_get_rect(cropRect, rect);
			var retStruct = new sk_imagefilter_croprect_get_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_croprect_get_flags_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_croprect_get_flags_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_flags_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_flags_0_Pre(parms);
			}
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_croprect_get_flags(cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_flags_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_croprect_get_flags_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_unref_0_Pre(parms);
			}
			var filter = parms.filter;
			var ret = CanvasKit._sk_imagefilter_unref(filter);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_matrix_0_Return();
			var parms = sk_imagefilter_new_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var quality = parms.quality;
			var input = parms.input;
			var ret = CanvasKit._sk_imagefilter_new_matrix(matrix, quality, input);
			var retStruct = new sk_imagefilter_new_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_alpha_threshold_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_alpha_threshold_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_alpha_threshold_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_alpha_threshold_0_Pre(parms);
			}
			var region = parms.region;
			var innerThreshold = parms.innerThreshold;
			var outerThreshold = parms.outerThreshold;
			var input = parms.input;
			var ret = CanvasKit._sk_imagefilter_new_alpha_threshold(region, innerThreshold, outerThreshold, input);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_alpha_threshold_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_alpha_threshold_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_blur_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_blur_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_blur_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_blur_0_Pre(parms);
			}
			var sigmaX = parms.sigmaX;
			var sigmaY = parms.sigmaY;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_blur(sigmaX, sigmaY, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_blur_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_blur_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_color_filter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_color_filter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_color_filter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_color_filter_0_Pre(parms);
			}
			var cf = parms.cf;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_color_filter(cf, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_color_filter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_color_filter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_compose_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_compose_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_compose_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_compose_0_Pre(parms);
			}
			var outer = parms.outer;
			var inner = parms.inner;
			var ret = CanvasKit._sk_imagefilter_new_compose(outer, inner);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_compose_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_compose_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_displacement_map_effect_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_displacement_map_effect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_displacement_map_effect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_displacement_map_effect_0_Pre(parms);
			}
			var xChannelSelector = parms.xChannelSelector;
			var yChannelSelector = parms.yChannelSelector;
			var scale = parms.scale;
			var displacement = parms.displacement;
			var color = parms.color;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_displacement_map_effect(xChannelSelector, yChannelSelector, scale, displacement, color, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_displacement_map_effect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_displacement_map_effect_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_drop_shadow_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_drop_shadow_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_drop_shadow_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_drop_shadow_0_Pre(parms);
			}
			var dx = parms.dx;
			var dy = parms.dy;
			var sigmaX = parms.sigmaX;
			var sigmaY = parms.sigmaY;
			var color = parms.color.color;
			var shadowMode = parms.shadowMode;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_drop_shadow(dx, dy, sigmaX, sigmaY, color, shadowMode, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_drop_shadow_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_drop_shadow_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_distant_lit_diffuse_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_distant_lit_diffuse_0_Return();
			var parms = sk_imagefilter_new_distant_lit_diffuse_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_diffuse_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_diffuse_0_Pre(parms);
			}
			var direction = parms.direction.marshalNew(CanvasKit);
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var kd = parms.kd;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_distant_lit_diffuse(direction, lightColor, surfaceScale, kd, input, cropRect);
			var retStruct = new sk_imagefilter_new_distant_lit_diffuse_0_Return();
			retStruct.direction = SkiaSharp.SKPoint3.unmarshal(direction, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_diffuse_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_diffuse_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_point_lit_diffuse_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_point_lit_diffuse_0_Return();
			var parms = sk_imagefilter_new_point_lit_diffuse_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_diffuse_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_diffuse_0_Pre(parms);
			}
			var location = parms.location.marshalNew(CanvasKit);
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var kd = parms.kd;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_point_lit_diffuse(location, lightColor, surfaceScale, kd, input, cropRect);
			var retStruct = new sk_imagefilter_new_point_lit_diffuse_0_Return();
			retStruct.location = SkiaSharp.SKPoint3.unmarshal(location, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_diffuse_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_diffuse_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_spot_lit_diffuse_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_spot_lit_diffuse_0_Return();
			var parms = sk_imagefilter_new_spot_lit_diffuse_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_diffuse_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_diffuse_0_Pre(parms);
			}
			var location = parms.location.marshalNew(CanvasKit);
			var target = parms.target.marshalNew(CanvasKit);
			var specularExponent = parms.specularExponent;
			var cutoffAngle = parms.cutoffAngle;
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var kd = parms.kd;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_spot_lit_diffuse(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, cropRect);
			var retStruct = new sk_imagefilter_new_spot_lit_diffuse_0_Return();
			retStruct.location = SkiaSharp.SKPoint3.unmarshal(location, CanvasKit);
			retStruct.target = SkiaSharp.SKPoint3.unmarshal(target, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_diffuse_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_diffuse_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_distant_lit_specular_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_distant_lit_specular_0_Return();
			var parms = sk_imagefilter_new_distant_lit_specular_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_specular_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_specular_0_Pre(parms);
			}
			var direction = parms.direction.marshalNew(CanvasKit);
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var ks = parms.ks;
			var shininess = parms.shininess;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_distant_lit_specular(direction, lightColor, surfaceScale, ks, shininess, input, cropRect);
			var retStruct = new sk_imagefilter_new_distant_lit_specular_0_Return();
			retStruct.direction = SkiaSharp.SKPoint3.unmarshal(direction, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_specular_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_distant_lit_specular_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_point_lit_specular_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_point_lit_specular_0_Return();
			var parms = sk_imagefilter_new_point_lit_specular_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_specular_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_specular_0_Pre(parms);
			}
			var location = parms.location.marshalNew(CanvasKit);
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var ks = parms.ks;
			var shininess = parms.shininess;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_point_lit_specular(location, lightColor, surfaceScale, ks, shininess, input, cropRect);
			var retStruct = new sk_imagefilter_new_point_lit_specular_0_Return();
			retStruct.location = SkiaSharp.SKPoint3.unmarshal(location, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_specular_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_point_lit_specular_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_spot_lit_specular_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_spot_lit_specular_0_Return();
			var parms = sk_imagefilter_new_spot_lit_specular_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_specular_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_specular_0_Pre(parms);
			}
			var location = parms.location.marshalNew(CanvasKit);
			var target = parms.target.marshalNew(CanvasKit);
			var specularExponent = parms.specularExponent;
			var cutoffAngle = parms.cutoffAngle;
			var lightColor = parms.lightColor.color;
			var surfaceScale = parms.surfaceScale;
			var ks = parms.ks;
			var shininess = parms.shininess;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_spot_lit_specular(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, cropRect);
			var retStruct = new sk_imagefilter_new_spot_lit_specular_0_Return();
			retStruct.location = SkiaSharp.SKPoint3.unmarshal(location, CanvasKit);
			retStruct.target = SkiaSharp.SKPoint3.unmarshal(target, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_specular_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_spot_lit_specular_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_magnifier_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_magnifier_0_Return();
			var parms = sk_imagefilter_new_magnifier_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_magnifier_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_magnifier_0_Pre(parms);
			}
			var src = parms.src.marshalNew(CanvasKit);
			var inset = parms.inset;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_magnifier(src, inset, input, cropRect);
			var retStruct = new sk_imagefilter_new_magnifier_0_Return();
			retStruct.src = SkiaSharp.SKRect.unmarshal(src, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_magnifier_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_magnifier_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_matrix_convolution_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_matrix_convolution_0_Return();
			var parms = sk_imagefilter_new_matrix_convolution_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_convolution_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_convolution_0_Pre(parms);
			}
			var kernelSize = parms.kernelSize.marshalNew(CanvasKit);
			var kernel = CanvasKit._malloc(parms.kernel_Length * 4); /*float*/
			var kernel_f32 = kernel / 4;
			
			{
				for(var i = 0; i < parms.kernel_Length; i++)
				{
					CanvasKit.HEAPF32[kernel_f32 + i] = parms.kernel[i];
				}
			}
			var gain = parms.gain;
			var bias = parms.bias;
			var kernelOffset = parms.kernelOffset.marshalNew(CanvasKit);
			var tileMode = parms.tileMode;
			var convolveAlpha = parms.convolveAlpha;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_matrix_convolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, cropRect);
			var retStruct = new sk_imagefilter_new_matrix_convolution_0_Return();
			retStruct.kernelSize = SkiaSharp.SKSizeI.unmarshal(kernelSize, CanvasKit);
			retStruct.kernelOffset = SkiaSharp.SKPointI.unmarshal(kernelOffset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_convolution_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_matrix_convolution_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_merge_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_merge_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_merge_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_merge_0_Pre(parms);
			}
			var filters = parms.filters; /* System.IntPtr */
			var count = parms.count;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_merge(filters, count, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_merge_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_merge_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_dilate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_dilate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_dilate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_dilate_0_Pre(parms);
			}
			var radiusX = parms.radiusX;
			var radiusY = parms.radiusY;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_dilate(radiusX, radiusY, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_dilate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_dilate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_erode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_erode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_erode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_erode_0_Pre(parms);
			}
			var radiusX = parms.radiusX;
			var radiusY = parms.radiusY;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_erode(radiusX, radiusY, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_erode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_erode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_offset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_offset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_offset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_offset_0_Pre(parms);
			}
			var dx = parms.dx;
			var dy = parms.dy;
			var input = parms.input;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_offset(dx, dy, input, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_offset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_offset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_picture_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_picture_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_0_Pre(parms);
			}
			var picture = parms.picture;
			var ret = CanvasKit._sk_imagefilter_new_picture(picture);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_picture_with_croprect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_picture_with_croprect_0_Return();
			var parms = sk_imagefilter_new_picture_with_croprect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_with_croprect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_with_croprect_0_Pre(parms);
			}
			var picture = parms.picture;
			var cropRect = parms.cropRect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_imagefilter_new_picture_with_croprect(picture, cropRect);
			var retStruct = new sk_imagefilter_new_picture_with_croprect_0_Return();
			retStruct.cropRect = SkiaSharp.SKRect.unmarshal(cropRect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_with_croprect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_picture_with_croprect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_tile_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_tile_0_Return();
			var parms = sk_imagefilter_new_tile_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_tile_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_tile_0_Pre(parms);
			}
			var src = parms.src.marshalNew(CanvasKit);
			var dst = parms.dst.marshalNew(CanvasKit);
			var input = parms.input;
			var ret = CanvasKit._sk_imagefilter_new_tile(src, dst, input);
			var retStruct = new sk_imagefilter_new_tile_0_Return();
			retStruct.src = SkiaSharp.SKRect.unmarshal(src, CanvasKit);
			retStruct.dst = SkiaSharp.SKRect.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_tile_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_tile_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_xfermode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_xfermode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_xfermode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_xfermode_0_Pre(parms);
			}
			var mode = parms.mode;
			var background = parms.background;
			var foreground = parms.foreground;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_xfermode(mode, background, foreground, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_xfermode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_xfermode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_arithmetic_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_arithmetic_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_arithmetic_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_arithmetic_0_Pre(parms);
			}
			var k1 = parms.k1;
			var k2 = parms.k2;
			var k3 = parms.k3;
			var k4 = parms.k4;
			var enforcePMColor = parms.enforcePMColor;
			var background = parms.background;
			var foreground = parms.foreground;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_arithmetic(k1, k2, k3, k4, enforcePMColor, background, foreground, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_arithmetic_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_arithmetic_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_image_source_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_imagefilter_new_image_source_0_Return();
			var parms = sk_imagefilter_new_image_source_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_0_Pre(parms);
			}
			var image = parms.image;
			var srcRect = parms.srcRect.marshalNew(CanvasKit);
			var dstRect = parms.dstRect.marshalNew(CanvasKit);
			var filterQuality = parms.filterQuality;
			var ret = CanvasKit._sk_imagefilter_new_image_source(image, srcRect, dstRect, filterQuality);
			var retStruct = new sk_imagefilter_new_image_source_0_Return();
			retStruct.srcRect = SkiaSharp.SKRect.unmarshal(srcRect, CanvasKit);
			retStruct.dstRect = SkiaSharp.SKRect.unmarshal(dstRect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_imagefilter_new_image_source_default_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_image_source_default_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_default_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_default_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_imagefilter_new_image_source_default(image);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_default_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_image_source_default_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_imagefilter_new_paint_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_imagefilter_new_paint_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_paint_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_imagefilter_new_paint_0_Pre(parms);
			}
			var paint = parms.paint;
			var cropRect = parms.cropRect;
			var ret = CanvasKit._sk_imagefilter_new_paint(paint, cropRect);
			if((<any>SkiaSharp.ApiOverride).sk_imagefilter_new_paint_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_imagefilter_new_paint_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_unref_0_Pre(parms);
			}
			var filter = parms.filter;
			var ret = CanvasKit._sk_colorfilter_unref(filter);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_mode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_mode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_mode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_mode_0_Pre(parms);
			}
			var c = parms.c.color;
			var mode = parms.mode;
			var ret = CanvasKit._sk_colorfilter_new_mode(c, mode);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_mode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_mode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_lighting_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_lighting_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_lighting_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_lighting_0_Pre(parms);
			}
			var mul = parms.mul.color;
			var add = parms.add.color;
			var ret = CanvasKit._sk_colorfilter_new_lighting(mul, add);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_lighting_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_lighting_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_compose_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_compose_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_compose_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_compose_0_Pre(parms);
			}
			var outer = parms.outer;
			var inner = parms.inner;
			var ret = CanvasKit._sk_colorfilter_new_compose(outer, inner);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_compose_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_compose_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_color_matrix_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_color_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_color_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_color_matrix_0_Pre(parms);
			}
			var array = CanvasKit._malloc(parms.array_Length * 4); /*float*/
			var array_f32 = array / 4;
			
			{
				for(var i = 0; i < parms.array_Length; i++)
				{
					CanvasKit.HEAPF32[array_f32 + i] = parms.array[i];
				}
			}
			var ret = CanvasKit._sk_colorfilter_new_color_matrix(array);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_color_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_color_matrix_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_luma_color_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_colorfilter_new_luma_color();
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_luma_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_luma_color_0_Post(ret);
			}
			return ret;
		}
		public static sk_colorfilter_new_table_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_table_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_0_Pre(parms);
			}
			var table = CanvasKit._malloc(parms.table_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.table_Length; i++)
				{
					CanvasKit.HEAPU8[table + i] = parms.table[i];
				}
			}
			var ret = CanvasKit._sk_colorfilter_new_table(table);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_table_argb_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colorfilter_new_table_argb_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_argb_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_argb_0_Pre(parms);
			}
			var tableA = CanvasKit._malloc(parms.tableA_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.tableA_Length; i++)
				{
					CanvasKit.HEAPU8[tableA + i] = parms.tableA[i];
				}
			}
			var tableR = CanvasKit._malloc(parms.tableR_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.tableR_Length; i++)
				{
					CanvasKit.HEAPU8[tableR + i] = parms.tableR[i];
				}
			}
			var tableG = CanvasKit._malloc(parms.tableG_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.tableG_Length; i++)
				{
					CanvasKit.HEAPU8[tableG + i] = parms.tableG[i];
				}
			}
			var tableB = CanvasKit._malloc(parms.tableB_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.tableB_Length; i++)
				{
					CanvasKit.HEAPU8[tableB + i] = parms.tableB[i];
				}
			}
			var ret = CanvasKit._sk_colorfilter_new_table_argb(tableA, tableR, tableG, tableB);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_argb_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_table_argb_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colorfilter_new_high_contrast_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colorfilter_new_high_contrast_0_Return();
			var parms = sk_colorfilter_new_high_contrast_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_high_contrast_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colorfilter_new_high_contrast_0_Pre(parms);
			}
			var config = parms.config.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_colorfilter_new_high_contrast(config);
			var retStruct = new sk_colorfilter_new_high_contrast_0_Return();
			retStruct.config = SkiaSharp.SKHighContrastConfig.unmarshal(config, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_colorfilter_new_high_contrast_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colorfilter_new_high_contrast_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_data_new_empty_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_data_new_empty();
			if((<any>SkiaSharp.ApiOverride).sk_data_new_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_empty_0_Post(ret);
			}
			return ret;
		}
		public static sk_data_new_with_copy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_with_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_0_Pre(parms);
			}
			var src = parms.src;
			var length = parms.length;
			var ret = CanvasKit._sk_data_new_with_copy(src, length);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_with_copy_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_with_copy_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_1_Pre(parms);
			}
			var src = CanvasKit._malloc(parms.src_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.src_Length; i++)
				{
					CanvasKit.HEAPU8[src + i] = parms.src[i];
				}
			}
			var length = parms.length;
			var ret = CanvasKit._sk_data_new_with_copy(src, length);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_with_copy_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_subset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_subset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_subset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_subset_0_Pre(parms);
			}
			var src = parms.src;
			var offset = parms.offset;
			var length = parms.length;
			var ret = CanvasKit._sk_data_new_subset(src, offset, length);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_subset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_subset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_from_file_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_from_file_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_from_file_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_from_file_0_Pre(parms);
			}
			var utf8path = CanvasKit._malloc(parms.utf8path_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.utf8path_Length; i++)
				{
					CanvasKit.HEAPU8[utf8path + i] = parms.utf8path[i];
				}
			}
			var ret = CanvasKit._sk_data_new_from_file(utf8path);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_from_file_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_from_file_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_from_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_from_stream_0_Pre(parms);
			}
			var stream = parms.stream;
			var length = parms.length;
			var ret = CanvasKit._sk_data_new_from_stream(stream, length);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_from_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_with_proc_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_with_proc_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_proc_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_with_proc_0_Pre(parms);
			}
			var ptr = parms.ptr;
			var length = parms.length;
			var proc = parms.proc;
			var ctx = parms.ctx;
			var ret = CanvasKit._sk_data_new_with_proc(ptr, length, proc, ctx);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_with_proc_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_with_proc_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_unref_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_data_unref(d);
			if((<any>SkiaSharp.ApiOverride).sk_data_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_get_size_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_get_size_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_get_size_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_get_size_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_data_get_size(d);
			if((<any>SkiaSharp.ApiOverride).sk_data_get_size_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_get_size_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_get_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_get_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_get_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_get_data_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_data_get_data(d);
			if((<any>SkiaSharp.ApiOverride).sk_data_get_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_get_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_data_new_uninitialized_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_data_new_uninitialized_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_uninitialized_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_data_new_uninitialized_0_Pre(parms);
			}
			var size = parms.size;
			var ret = CanvasKit._sk_data_new_uninitialized(size);
			if((<any>SkiaSharp.ApiOverride).sk_data_new_uninitialized_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_data_new_uninitialized_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_string_new_empty_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_string_new_empty();
			if((<any>SkiaSharp.ApiOverride).sk_string_new_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_string_new_empty_0_Post(ret);
			}
			return ret;
		}
		public static sk_string_new_with_copy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_string_new_with_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_string_new_with_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_string_new_with_copy_0_Pre(parms);
			}
			var src = CanvasKit._malloc(parms.src_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.src_Length; i++)
				{
					CanvasKit.HEAPU8[src + i] = parms.src[i];
				}
			}
			var length = parms.length;
			var ret = CanvasKit._sk_string_new_with_copy(src, length);
			if((<any>SkiaSharp.ApiOverride).sk_string_new_with_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_string_new_with_copy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_string_destructor_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_string_destructor_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_string_destructor_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_string_destructor_0_Pre(parms);
			}
			var skstring = parms.skstring;
			var ret = CanvasKit._sk_string_destructor(skstring);
			if((<any>SkiaSharp.ApiOverride).sk_string_destructor_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_string_destructor_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_string_get_size_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_string_get_size_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_string_get_size_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_string_get_size_0_Pre(parms);
			}
			var skstring = parms.skstring;
			var ret = CanvasKit._sk_string_get_size(skstring);
			if((<any>SkiaSharp.ApiOverride).sk_string_get_size_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_string_get_size_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_string_get_c_str_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_string_get_c_str_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_string_get_c_str_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_string_get_c_str_0_Pre(parms);
			}
			var skstring = parms.skstring;
			var ret = CanvasKit._sk_string_get_c_str(skstring);
			if((<any>SkiaSharp.ApiOverride).sk_string_get_c_str_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_string_get_c_str_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_recorder_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_recorder_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_recorder_delete_0_Pre(parms);
			}
			var r = parms.r;
			var ret = CanvasKit._sk_picture_recorder_delete(r);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_recorder_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_recorder_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_picture_recorder_new();
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_recorder_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_picture_recorder_begin_recording_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_picture_recorder_begin_recording_0_Return();
			var parms = sk_picture_recorder_begin_recording_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_begin_recording_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_recorder_begin_recording_0_Pre(parms);
			}
			var r = parms.r;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_picture_recorder_begin_recording(r, rect);
			var retStruct = new sk_picture_recorder_begin_recording_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_begin_recording_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_recorder_begin_recording_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_picture_recorder_end_recording_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_recorder_end_recording_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_0_Pre(parms);
			}
			var r = parms.r;
			var ret = CanvasKit._sk_picture_recorder_end_recording(r);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_recorder_end_recording_as_drawable_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_recorder_end_recording_as_drawable_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_as_drawable_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_as_drawable_0_Pre(parms);
			}
			var r = parms.r;
			var ret = CanvasKit._sk_picture_recorder_end_recording_as_drawable(r);
			if((<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_as_drawable_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_recorder_end_recording_as_drawable_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_get_recording_canvas_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_get_recording_canvas_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_recording_canvas_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_get_recording_canvas_0_Pre(parms);
			}
			var r = parms.r;
			var ret = CanvasKit._sk_picture_get_recording_canvas(r);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_recording_canvas_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_get_recording_canvas_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_unref_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_picture_unref(t);
			if((<any>SkiaSharp.ApiOverride).sk_picture_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_get_unique_id_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_picture_get_unique_id_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_unique_id_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_get_unique_id_0_Pre(parms);
			}
			var p = parms.p;
			var ret = CanvasKit._sk_picture_get_unique_id(p);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_unique_id_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_get_unique_id_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_picture_get_cull_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_picture_get_cull_rect_0_Return();
			var parms = sk_picture_get_cull_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_cull_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_picture_get_cull_rect_0_Pre(parms);
			}
			var p = parms.p;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_picture_get_cull_rect(p, rect);
			var retStruct = new sk_picture_get_cull_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_picture_get_cull_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_picture_get_cull_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_manageddrawable_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_manageddrawable_new();
			if((<any>SkiaSharp.ApiOverride).sk_manageddrawable_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_manageddrawable_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_manageddrawable_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_manageddrawable_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_manageddrawable_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_manageddrawable_destroy_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_manageddrawable_destroy(t);
			if((<any>SkiaSharp.ApiOverride).sk_manageddrawable_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_manageddrawable_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_manageddrawable_set_delegates_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_manageddrawable_set_delegates_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_manageddrawable_set_delegates_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_manageddrawable_set_delegates_0_Pre(parms);
			}
			var pDraw = parms.pDraw;
			var pGetBounds = parms.pGetBounds;
			var pNewPictureSnapshot = parms.pNewPictureSnapshot;
			var ret = CanvasKit._sk_manageddrawable_set_delegates(pDraw, pGetBounds, pNewPictureSnapshot);
			if((<any>SkiaSharp.ApiOverride).sk_manageddrawable_set_delegates_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_manageddrawable_set_delegates_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_drawable_get_generation_id_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_drawable_get_generation_id_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_get_generation_id_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_drawable_get_generation_id_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_drawable_get_generation_id(d);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_get_generation_id_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_drawable_get_generation_id_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_drawable_get_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_drawable_get_bounds_0_Return();
			var parms = sk_drawable_get_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_get_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_drawable_get_bounds_0_Pre(parms);
			}
			var d = parms.d;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_drawable_get_bounds(d, rect);
			var retStruct = new sk_drawable_get_bounds_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_get_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_drawable_get_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_drawable_draw_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_drawable_draw_0_Return();
			var parms = sk_drawable_draw_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_draw_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_drawable_draw_0_Pre(parms);
			}
			var d = parms.d;
			var c = parms.c;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_drawable_draw(d, c, matrix);
			var retStruct = new sk_drawable_draw_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_draw_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_drawable_draw_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_drawable_new_picture_snapshot_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_drawable_new_picture_snapshot_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_new_picture_snapshot_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_drawable_new_picture_snapshot_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_drawable_new_picture_snapshot(d);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_new_picture_snapshot_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_drawable_new_picture_snapshot_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_drawable_notify_drawing_changed_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_drawable_notify_drawing_changed_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_notify_drawing_changed_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_drawable_notify_drawing_changed_0_Pre(parms);
			}
			var d = parms.d;
			var ret = CanvasKit._sk_drawable_notify_drawing_changed(d);
			if((<any>SkiaSharp.ApiOverride).sk_drawable_notify_drawing_changed_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_drawable_notify_drawing_changed_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_unref_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_shader_unref(t);
			if((<any>SkiaSharp.ApiOverride).sk_shader_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_empty_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_shader_new_empty();
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_empty_0_Post(ret);
			}
			return ret;
		}
		public static sk_shader_new_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_color_0_Pre(parms);
			}
			var color = parms.color.color;
			var ret = CanvasKit._sk_shader_new_color(color);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_local_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_local_matrix_0_Return();
			var parms = sk_shader_new_local_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_local_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_local_matrix_0_Pre(parms);
			}
			var proxy = parms.proxy;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_local_matrix(proxy, matrix);
			var retStruct = new sk_shader_new_local_matrix_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_local_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_local_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_color_filter_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_color_filter_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_color_filter_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_color_filter_0_Pre(parms);
			}
			var proxy = parms.proxy;
			var filter = parms.filter;
			var ret = CanvasKit._sk_shader_new_color_filter(proxy, filter);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_color_filter_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_color_filter_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_bitmap_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_bitmap_0_Return();
			var parms = sk_shader_new_bitmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_0_Pre(parms);
			}
			var src = parms.src;
			var tmx = parms.tmx;
			var tmy = parms.tmy;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_bitmap(src, tmx, tmy, matrix);
			var retStruct = new sk_shader_new_bitmap_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_bitmap_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_bitmap_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_1_Pre(parms);
			}
			var src = parms.src;
			var tmx = parms.tmx;
			var tmy = parms.tmy;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_bitmap(src, tmx, tmy, matrixZero);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_bitmap_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_linear_gradient_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_linear_gradient_0_Return();
			var parms = sk_shader_new_linear_gradient_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_0_Pre(parms);
			}
			var points = parms.points;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_linear_gradient(points, colors, colorPos, count, mode, matrix);
			var retStruct = new sk_shader_new_linear_gradient_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_linear_gradient_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_linear_gradient_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_1_Pre(parms);
			}
			var points = parms.points;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_linear_gradient(points, colors, colorPos, count, mode, matrixZero);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_linear_gradient_2(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_linear_gradient_2_Return();
			var parms = sk_shader_new_linear_gradient_2_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_2_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_2_Pre(parms);
			}
			var points = parms.points;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_linear_gradient(points, colors, colorPosZero, count, mode, matrix);
			var retStruct = new sk_shader_new_linear_gradient_2_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_2_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_2_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_linear_gradient_3(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_linear_gradient_3_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_3_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_3_Pre(parms);
			}
			var points = parms.points;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_linear_gradient(points, colors, colorPosZero, count, mode, matrixZero);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_3_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_linear_gradient_3_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_radial_gradient_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_radial_gradient_0_Return();
			var parms = sk_shader_new_radial_gradient_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_0_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var radius = parms.radius;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_radial_gradient(center, radius, colors, colorPos, count, mode, matrix);
			var retStruct = new sk_shader_new_radial_gradient_0_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_radial_gradient_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_radial_gradient_1_Return();
			var parms = sk_shader_new_radial_gradient_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_1_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var radius = parms.radius;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_radial_gradient(center, radius, colors, colorPos, count, mode, matrixZero);
			var retStruct = new sk_shader_new_radial_gradient_1_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_radial_gradient_2(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_radial_gradient_2_Return();
			var parms = sk_shader_new_radial_gradient_2_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_2_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_2_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var radius = parms.radius;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_radial_gradient(center, radius, colors, colorPosZero, count, mode, matrix);
			var retStruct = new sk_shader_new_radial_gradient_2_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_2_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_2_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_radial_gradient_3(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_radial_gradient_3_Return();
			var parms = sk_shader_new_radial_gradient_3_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_3_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_3_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var radius = parms.radius;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_radial_gradient(center, radius, colors, colorPosZero, count, mode, matrixZero);
			var retStruct = new sk_shader_new_radial_gradient_3_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_3_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_radial_gradient_3_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_sweep_gradient_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_sweep_gradient_0_Return();
			var parms = sk_shader_new_sweep_gradient_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_0_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var startAngle = parms.startAngle;
			var endAngle = parms.endAngle;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_sweep_gradient(center, colors, colorPos, count, mode, startAngle, endAngle, matrixZero);
			var retStruct = new sk_shader_new_sweep_gradient_0_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_sweep_gradient_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_sweep_gradient_1_Return();
			var parms = sk_shader_new_sweep_gradient_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_1_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var startAngle = parms.startAngle;
			var endAngle = parms.endAngle;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_sweep_gradient(center, colors, colorPos, count, mode, startAngle, endAngle, matrix);
			var retStruct = new sk_shader_new_sweep_gradient_1_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_sweep_gradient_2(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_sweep_gradient_2_Return();
			var parms = sk_shader_new_sweep_gradient_2_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_2_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_2_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var startAngle = parms.startAngle;
			var endAngle = parms.endAngle;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_sweep_gradient(center, colors, colorPosZero, count, mode, startAngle, endAngle, matrixZero);
			var retStruct = new sk_shader_new_sweep_gradient_2_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_2_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_2_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_sweep_gradient_3(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_sweep_gradient_3_Return();
			var parms = sk_shader_new_sweep_gradient_3_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_3_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_3_Pre(parms);
			}
			var center = parms.center.marshalNew(CanvasKit);
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var startAngle = parms.startAngle;
			var endAngle = parms.endAngle;
			var matrixZero = parms.matrixZero.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_sweep_gradient(center, colors, colorPosZero, count, mode, startAngle, endAngle, matrixZero);
			var retStruct = new sk_shader_new_sweep_gradient_3_Return();
			retStruct.center = SkiaSharp.SKPoint.unmarshal(center, CanvasKit);
			retStruct.matrixZero = SkiaSharp.SKMatrix.unmarshal(matrixZero, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_3_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_sweep_gradient_3_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_two_point_conical_gradient_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_two_point_conical_gradient_0_Return();
			var parms = sk_shader_new_two_point_conical_gradient_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_0_Pre(parms);
			}
			var start = parms.start.marshalNew(CanvasKit);
			var startRadius = parms.startRadius;
			var end = parms.end.marshalNew(CanvasKit);
			var endRadius = parms.endRadius;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_two_point_conical_gradient(start, startRadius, end, endRadius, colors, colorPos, count, mode, matrix);
			var retStruct = new sk_shader_new_two_point_conical_gradient_0_Return();
			retStruct.start = SkiaSharp.SKPoint.unmarshal(start, CanvasKit);
			retStruct.end = SkiaSharp.SKPoint.unmarshal(end, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_two_point_conical_gradient_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_two_point_conical_gradient_1_Return();
			var parms = sk_shader_new_two_point_conical_gradient_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_1_Pre(parms);
			}
			var start = parms.start.marshalNew(CanvasKit);
			var startRadius = parms.startRadius;
			var end = parms.end.marshalNew(CanvasKit);
			var endRadius = parms.endRadius;
			var colors = parms.colors;
			var colorPos = CanvasKit._malloc(parms.colorPos_Length * 4); /*float*/
			var colorPos_f32 = colorPos / 4;
			
			{
				for(var i = 0; i < parms.colorPos_Length; i++)
				{
					CanvasKit.HEAPF32[colorPos_f32 + i] = parms.colorPos[i];
				}
			}
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_two_point_conical_gradient(start, startRadius, end, endRadius, colors, colorPos, count, mode, matrixZero);
			var retStruct = new sk_shader_new_two_point_conical_gradient_1_Return();
			retStruct.start = SkiaSharp.SKPoint.unmarshal(start, CanvasKit);
			retStruct.end = SkiaSharp.SKPoint.unmarshal(end, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_two_point_conical_gradient_2(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_two_point_conical_gradient_2_Return();
			var parms = sk_shader_new_two_point_conical_gradient_2_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_2_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_2_Pre(parms);
			}
			var start = parms.start.marshalNew(CanvasKit);
			var startRadius = parms.startRadius;
			var end = parms.end.marshalNew(CanvasKit);
			var endRadius = parms.endRadius;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_two_point_conical_gradient(start, startRadius, end, endRadius, colors, colorPosZero, count, mode, matrix);
			var retStruct = new sk_shader_new_two_point_conical_gradient_2_Return();
			retStruct.start = SkiaSharp.SKPoint.unmarshal(start, CanvasKit);
			retStruct.end = SkiaSharp.SKPoint.unmarshal(end, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_2_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_2_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_two_point_conical_gradient_3(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_two_point_conical_gradient_3_Return();
			var parms = sk_shader_new_two_point_conical_gradient_3_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_3_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_3_Pre(parms);
			}
			var start = parms.start.marshalNew(CanvasKit);
			var startRadius = parms.startRadius;
			var end = parms.end.marshalNew(CanvasKit);
			var endRadius = parms.endRadius;
			var colors = parms.colors;
			var colorPosZero = parms.colorPosZero;
			var count = parms.count;
			var mode = parms.mode;
			var matrixZero = parms.matrixZero;
			var ret = CanvasKit._sk_shader_new_two_point_conical_gradient(start, startRadius, end, endRadius, colors, colorPosZero, count, mode, matrixZero);
			var retStruct = new sk_shader_new_two_point_conical_gradient_3_Return();
			retStruct.start = SkiaSharp.SKPoint.unmarshal(start, CanvasKit);
			retStruct.end = SkiaSharp.SKPoint.unmarshal(end, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_3_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_two_point_conical_gradient_3_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_perlin_noise_fractal_noise_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_perlin_noise_fractal_noise_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_0_Pre(parms);
			}
			var baseFrequencyX = parms.baseFrequencyX;
			var baseFrequencyY = parms.baseFrequencyY;
			var numOctaves = parms.numOctaves;
			var seed = parms.seed;
			var tileSizeZero = parms.tileSizeZero;
			var ret = CanvasKit._sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSizeZero);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_perlin_noise_fractal_noise_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_perlin_noise_fractal_noise_1_Return();
			var parms = sk_shader_new_perlin_noise_fractal_noise_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_1_Pre(parms);
			}
			var baseFrequencyX = parms.baseFrequencyX;
			var baseFrequencyY = parms.baseFrequencyY;
			var numOctaves = parms.numOctaves;
			var seed = parms.seed;
			var tileSize = parms.tileSize.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
			var retStruct = new sk_shader_new_perlin_noise_fractal_noise_1_Return();
			retStruct.tileSize = SkiaSharp.SKPointI.unmarshal(tileSize, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_fractal_noise_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_perlin_noise_turbulence_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_perlin_noise_turbulence_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_0_Pre(parms);
			}
			var baseFrequencyX = parms.baseFrequencyX;
			var baseFrequencyY = parms.baseFrequencyY;
			var numOctaves = parms.numOctaves;
			var seed = parms.seed;
			var tileSizeZero = parms.tileSizeZero;
			var ret = CanvasKit._sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSizeZero);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_perlin_noise_turbulence_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_shader_new_perlin_noise_turbulence_1_Return();
			var parms = sk_shader_new_perlin_noise_turbulence_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_1_Pre(parms);
			}
			var baseFrequencyX = parms.baseFrequencyX;
			var baseFrequencyY = parms.baseFrequencyY;
			var numOctaves = parms.numOctaves;
			var seed = parms.seed;
			var tileSize = parms.tileSize.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
			var retStruct = new sk_shader_new_perlin_noise_turbulence_1_Return();
			retStruct.tileSize = SkiaSharp.SKPointI.unmarshal(tileSize, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_perlin_noise_turbulence_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_shader_new_compose_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_compose_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_compose_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_compose_0_Pre(parms);
			}
			var shaderA = parms.shaderA;
			var shaderB = parms.shaderB;
			var ret = CanvasKit._sk_shader_new_compose(shaderA, shaderB);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_compose_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_compose_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_shader_new_compose_with_mode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_shader_new_compose_with_mode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_compose_with_mode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_shader_new_compose_with_mode_0_Pre(parms);
			}
			var shaderA = parms.shaderA;
			var shaderB = parms.shaderB;
			var mode = parms.mode;
			var ret = CanvasKit._sk_shader_new_compose_with_mode(shaderA, shaderB, mode);
			if((<any>SkiaSharp.ApiOverride).sk_shader_new_compose_with_mode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_shader_new_compose_with_mode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_create_default_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_typeface_create_default();
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_default_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_create_default_0_Post(ret);
			}
			return ret;
		}
		public static sk_typeface_ref_default_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_typeface_ref_default();
			if((<any>SkiaSharp.ApiOverride).sk_typeface_ref_default_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_ref_default_0_Post(ret);
			}
			return ret;
		}
		public static sk_typeface_create_from_name_with_font_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_create_from_name_with_font_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_name_with_font_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_create_from_name_with_font_style_0_Pre(parms);
			}
			var familyName_Length = parms.familyName.length*4+1
			var familyName = CanvasKit._malloc(familyName_Length);
			CanvasKit.stringToUTF8(parms.familyName, familyName, familyName_Length);
			var style = parms.style;
			var ret = CanvasKit._sk_typeface_create_from_name_with_font_style(familyName, style);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_name_with_font_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_create_from_name_with_font_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_create_from_file_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_create_from_file_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_file_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_create_from_file_0_Pre(parms);
			}
			var utf8path = CanvasKit._malloc(parms.utf8path_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.utf8path_Length; i++)
				{
					CanvasKit.HEAPU8[utf8path + i] = parms.utf8path[i];
				}
			}
			var index = parms.index;
			var ret = CanvasKit._sk_typeface_create_from_file(utf8path, index);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_file_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_create_from_file_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_create_from_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_create_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_create_from_stream_0_Pre(parms);
			}
			var stream = parms.stream;
			var index = parms.index;
			var ret = CanvasKit._sk_typeface_create_from_stream(stream, index);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_create_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_create_from_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_unref_0_Pre(parms);
			}
			var t = parms.t;
			var ret = CanvasKit._sk_typeface_unref(t);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_chars_to_glyphs_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_chars_to_glyphs_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_chars_to_glyphs_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_chars_to_glyphs_0_Pre(parms);
			}
			var t = parms.t;
			var chars = parms.chars;
			var encoding = parms.encoding;
			var glyphPtr = parms.glyphPtr;
			var glyphCount = parms.glyphCount;
			var ret = CanvasKit._sk_typeface_chars_to_glyphs(t, chars, encoding, glyphPtr, glyphCount);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_chars_to_glyphs_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_chars_to_glyphs_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_family_name_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_family_name_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_family_name_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_family_name_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_family_name(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_family_name_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_family_name_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_count_tables_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_count_tables_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_count_tables_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_count_tables_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_count_tables(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_count_tables_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_count_tables_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_table_tags_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_table_tags_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_tags_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_table_tags_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var tags = parms.tags; /* uint */
			var ret = CanvasKit._sk_typeface_get_table_tags(typeface, tags);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_tags_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_table_tags_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_table_size_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_table_size_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_size_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_table_size_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var tag = parms.tag;
			var ret = CanvasKit._sk_typeface_get_table_size(typeface, tag);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_size_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_table_size_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_table_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_table_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_table_data_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var tag = parms.tag;
			var offset = parms.offset;
			var length = parms.length;
			var data = CanvasKit._malloc(parms.data_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.data_Length; i++)
				{
					CanvasKit.HEAPU8[data + i] = parms.data[i];
				}
			}
			var ret = CanvasKit._sk_typeface_get_table_data(typeface, tag, offset, length, data);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_table_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_table_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_fontstyle_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_fontstyle_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_fontstyle_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_fontstyle_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_fontstyle(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_fontstyle_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_fontstyle_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_font_weight_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_font_weight_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_weight_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_font_weight_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_font_weight(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_weight_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_font_weight_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_font_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_font_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_font_width_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_font_width(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_font_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_get_font_slant_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_font_slant_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_slant_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_font_slant_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_font_slant(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_font_slant_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_font_slant_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_typeface_open_stream_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_typeface_open_stream_0_Return();
			var parms = sk_typeface_open_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_open_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_open_stream_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ttcIndex = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_typeface_open_stream(typeface, ttcIndex);
			var retStruct = new sk_typeface_open_stream_0_Return();
			retStruct.ttcIndex = CanvasKit.getValue(ttcIndex, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_typeface_open_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_open_stream_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_typeface_get_units_per_em_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_typeface_get_units_per_em_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_units_per_em_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_typeface_get_units_per_em_0_Pre(parms);
			}
			var typeface = parms.typeface;
			var ret = CanvasKit._sk_typeface_get_units_per_em(typeface);
			if((<any>SkiaSharp.ApiOverride).sk_typeface_get_units_per_em_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_typeface_get_units_per_em_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_create_default_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_fontmgr_create_default();
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_default_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_create_default_0_Post(ret);
			}
			return ret;
		}
		public static sk_fontmgr_ref_default_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_fontmgr_ref_default();
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_ref_default_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_ref_default_0_Post(ret);
			}
			return ret;
		}
		public static sk_fontmgr_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_unref_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var ret = CanvasKit._sk_fontmgr_unref(fontmgr);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_count_families_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_count_families_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_count_families_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_count_families_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var ret = CanvasKit._sk_fontmgr_count_families(fontmgr);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_count_families_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_count_families_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_get_family_name_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_get_family_name_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_get_family_name_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_get_family_name_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var index = parms.index;
			var familyName = parms.familyName;
			var ret = CanvasKit._sk_fontmgr_get_family_name(fontmgr, index, familyName);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_get_family_name_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_get_family_name_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_match_family_style_character_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_match_family_style_character_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_character_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_character_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var familyName_Length = parms.familyName.length*4+1
			var familyName = CanvasKit._malloc(familyName_Length);
			CanvasKit.stringToUTF8(parms.familyName, familyName, familyName_Length);
			var style = parms.style;
			var bcp47 = parms.bcp47; /* string */
			var bcp47Count = parms.bcp47Count;
			var character = parms.character;
			var ret = CanvasKit._sk_fontmgr_match_family_style_character(fontmgr, familyName, style, bcp47, bcp47Count, character);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_character_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_character_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_create_styleset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_create_styleset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_styleset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_create_styleset_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var index = parms.index;
			var ret = CanvasKit._sk_fontmgr_create_styleset(fontmgr, index);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_styleset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_create_styleset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_match_family_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_match_family_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var familyName_Length = parms.familyName.length*4+1
			var familyName = CanvasKit._malloc(familyName_Length);
			CanvasKit.stringToUTF8(parms.familyName, familyName, familyName_Length);
			var ret = CanvasKit._sk_fontmgr_match_family(fontmgr, familyName);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_match_family_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_match_family_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var familyName_Length = parms.familyName.length*4+1
			var familyName = CanvasKit._malloc(familyName_Length);
			CanvasKit.stringToUTF8(parms.familyName, familyName, familyName_Length);
			var style = parms.style;
			var ret = CanvasKit._sk_fontmgr_match_family_style(fontmgr, familyName, style);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_match_family_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_match_face_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_match_face_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_face_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_match_face_style_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var face = parms.face;
			var style = parms.style;
			var ret = CanvasKit._sk_fontmgr_match_face_style(fontmgr, face, style);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_match_face_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_match_face_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_create_from_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_create_from_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_data_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var data = parms.data;
			var index = parms.index;
			var ret = CanvasKit._sk_fontmgr_create_from_data(fontmgr, data, index);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_create_from_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_create_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_stream_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var stream = parms.stream;
			var index = parms.index;
			var ret = CanvasKit._sk_fontmgr_create_from_stream(fontmgr, stream, index);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontmgr_create_from_file_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontmgr_create_from_file_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_file_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_file_0_Pre(parms);
			}
			var fontmgr = parms.fontmgr;
			var utf8path = CanvasKit._malloc(parms.utf8path_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.utf8path_Length; i++)
				{
					CanvasKit.HEAPU8[utf8path + i] = parms.utf8path[i];
				}
			}
			var index = parms.index;
			var ret = CanvasKit._sk_fontmgr_create_from_file(fontmgr, utf8path, index);
			if((<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_file_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontmgr_create_from_file_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyle_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyle_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyle_new_0_Pre(parms);
			}
			var weight = parms.weight;
			var width = parms.width;
			var slant = parms.slant;
			var ret = CanvasKit._sk_fontstyle_new(weight, width, slant);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyle_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyle_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyle_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyle_delete_0_Pre(parms);
			}
			var fs = parms.fs;
			var ret = CanvasKit._sk_fontstyle_delete(fs);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyle_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyle_get_weight_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyle_get_weight_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_weight_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyle_get_weight_0_Pre(parms);
			}
			var fs = parms.fs;
			var ret = CanvasKit._sk_fontstyle_get_weight(fs);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_weight_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyle_get_weight_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyle_get_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyle_get_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyle_get_width_0_Pre(parms);
			}
			var fs = parms.fs;
			var ret = CanvasKit._sk_fontstyle_get_width(fs);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyle_get_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyle_get_slant_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyle_get_slant_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_slant_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyle_get_slant_0_Pre(parms);
			}
			var fs = parms.fs;
			var ret = CanvasKit._sk_fontstyle_get_slant(fs);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyle_get_slant_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyle_get_slant_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyleset_create_empty_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_fontstyleset_create_empty();
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_empty_0_Post(ret);
			}
			return ret;
		}
		public static sk_fontstyleset_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyleset_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyleset_unref_0_Pre(parms);
			}
			var fss = parms.fss;
			var ret = CanvasKit._sk_fontstyleset_unref(fss);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyleset_get_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyleset_get_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_count_0_Pre(parms);
			}
			var fss = parms.fss;
			var ret = CanvasKit._sk_fontstyleset_get_count(fss);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyleset_get_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyleset_get_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_style_0_Pre(parms);
			}
			var fss = parms.fss;
			var index = parms.index;
			var fs = parms.fs;
			var style = parms.style;
			var ret = CanvasKit._sk_fontstyleset_get_style(fss, index, fs, style);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_get_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyleset_create_typeface_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyleset_create_typeface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_typeface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_typeface_0_Pre(parms);
			}
			var fss = parms.fss;
			var index = parms.index;
			var ret = CanvasKit._sk_fontstyleset_create_typeface(fss, index);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_typeface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_create_typeface_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_fontstyleset_match_style_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_fontstyleset_match_style_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_match_style_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_fontstyleset_match_style_0_Pre(parms);
			}
			var fss = parms.fss;
			var style = parms.style;
			var ret = CanvasKit._sk_fontstyleset_match_style(fss, style);
			if((<any>SkiaSharp.ApiOverride).sk_fontstyleset_match_style_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_fontstyleset_match_style_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_destroy_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_memorystream_destroy(stream);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filestream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filestream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filestream_destroy_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_filestream_destroy(stream);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filestream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_asset_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_asset_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_asset_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_asset_destroy_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_asset_destroy(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_asset_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_asset_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_read_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_read_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = parms.buffer;
			var size = parms.size;
			var ret = CanvasKit._sk_stream_read(stream, buffer, size);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_peek_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_peek_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_peek_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_peek_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = parms.buffer;
			var size = parms.size;
			var ret = CanvasKit._sk_stream_peek(stream, buffer, size);
			if((<any>SkiaSharp.ApiOverride).sk_stream_peek_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_peek_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_skip_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_skip_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_skip_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_skip_0_Pre(parms);
			}
			var stream = parms.stream;
			var size = parms.size;
			var ret = CanvasKit._sk_stream_skip(stream, size);
			if((<any>SkiaSharp.ApiOverride).sk_stream_skip_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_skip_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_is_at_end_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_is_at_end_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_is_at_end_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_is_at_end_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_is_at_end(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_is_at_end_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_is_at_end_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_read_s8_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_s8_0_Return();
			var parms = sk_stream_read_s8_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s8_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_s8_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(1);
			var ret = CanvasKit._sk_stream_read_s8(stream, buffer);
			var retStruct = new sk_stream_read_s8_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i8");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s8_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_s8_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_s16_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_s16_0_Return();
			var parms = sk_stream_read_s16_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s16_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_s16_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(2);
			var ret = CanvasKit._sk_stream_read_s16(stream, buffer);
			var retStruct = new sk_stream_read_s16_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i16");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s16_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_s16_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_s32_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_s32_0_Return();
			var parms = sk_stream_read_s32_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s32_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_s32_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_stream_read_s32(stream, buffer);
			var retStruct = new sk_stream_read_s32_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_s32_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_s32_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_u8_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_u8_0_Return();
			var parms = sk_stream_read_u8_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u8_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_u8_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(1);
			var ret = CanvasKit._sk_stream_read_u8(stream, buffer);
			var retStruct = new sk_stream_read_u8_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i8");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u8_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_u8_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_u16_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_u16_0_Return();
			var parms = sk_stream_read_u16_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u16_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_u16_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(2);
			var ret = CanvasKit._sk_stream_read_u16(stream, buffer);
			var retStruct = new sk_stream_read_u16_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i16");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u16_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_u16_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_u32_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_u32_0_Return();
			var parms = sk_stream_read_u32_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u32_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_u32_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_stream_read_u32(stream, buffer);
			var retStruct = new sk_stream_read_u32_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_u32_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_u32_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_read_bool_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_stream_read_bool_0_Return();
			var parms = sk_stream_read_bool_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_bool_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_read_bool_0_Pre(parms);
			}
			var stream = parms.stream;
			var buffer = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_stream_read_bool(stream, buffer);
			var retStruct = new sk_stream_read_bool_0_Return();
			retStruct.buffer = CanvasKit.getValue(buffer, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_stream_read_bool_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_read_bool_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_stream_rewind_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_rewind_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_rewind_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_rewind_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_rewind(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_rewind_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_rewind_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_has_position_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_has_position_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_has_position_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_has_position_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_has_position(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_has_position_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_has_position_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_get_position_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_get_position_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_position_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_get_position_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_get_position(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_position_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_get_position_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_seek_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_seek_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_seek_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_seek_0_Pre(parms);
			}
			var stream = parms.stream;
			var position = parms.position;
			var ret = CanvasKit._sk_stream_seek(stream, position);
			if((<any>SkiaSharp.ApiOverride).sk_stream_seek_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_seek_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_move_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_move_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_move_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_move_0_Pre(parms);
			}
			var stream = parms.stream;
			var offset = parms.offset;
			var ret = CanvasKit._sk_stream_move(stream, offset);
			if((<any>SkiaSharp.ApiOverride).sk_stream_move_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_move_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_has_length_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_has_length_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_has_length_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_has_length_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_has_length(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_has_length_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_has_length_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_get_length_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_get_length_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_length_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_get_length_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_stream_get_length(stream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_length_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_get_length_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_stream_get_memory_base_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_stream_get_memory_base_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_memory_base_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_stream_get_memory_base_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_stream_get_memory_base(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_stream_get_memory_base_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_stream_get_memory_base_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filestream_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filestream_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filestream_new_0_Pre(parms);
			}
			var utf8path = CanvasKit._malloc(parms.utf8path_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.utf8path_Length; i++)
				{
					CanvasKit.HEAPU8[utf8path + i] = parms.utf8path[i];
				}
			}
			var ret = CanvasKit._sk_filestream_new(utf8path);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filestream_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_memorystream_new();
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_memorystream_new_with_length_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_new_with_length_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_length_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_length_0_Pre(parms);
			}
			var length = parms.length;
			var ret = CanvasKit._sk_memorystream_new_with_length(length);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_length_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_length_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_new_with_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_new_with_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_0_Pre(parms);
			}
			var data = parms.data;
			var length = parms.length;
			var copyData = parms.copyData;
			var ret = CanvasKit._sk_memorystream_new_with_data(data, length, copyData);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_new_with_data_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_new_with_data_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_1_Pre(parms);
			}
			var data = CanvasKit._malloc(parms.data_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.data_Length; i++)
				{
					CanvasKit.HEAPU8[data + i] = parms.data[i];
				}
			}
			var length = parms.length;
			var copyData = parms.copyData;
			var ret = CanvasKit._sk_memorystream_new_with_data(data, length, copyData);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_data_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_new_with_skdata_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_new_with_skdata_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_skdata_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_skdata_0_Pre(parms);
			}
			var data = parms.data;
			var ret = CanvasKit._sk_memorystream_new_with_skdata(data);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_skdata_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_new_with_skdata_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_set_memory_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_set_memory_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_0_Pre(parms);
			}
			var s = parms.s;
			var data = parms.data;
			var length = parms.length;
			var copyData = parms.copyData;
			var ret = CanvasKit._sk_memorystream_set_memory(s, data, length, copyData);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_memorystream_set_memory_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_memorystream_set_memory_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_1_Pre(parms);
			}
			var s = parms.s;
			var data = CanvasKit._malloc(parms.data_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.data_Length; i++)
				{
					CanvasKit.HEAPU8[data + i] = parms.data[i];
				}
			}
			var length = parms.length;
			var copyData = parms.copyData;
			var ret = CanvasKit._sk_memorystream_set_memory(s, data, length, copyData);
			if((<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_memorystream_set_memory_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filestream_is_valid_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filestream_is_valid_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_is_valid_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filestream_is_valid_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_filestream_is_valid(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_filestream_is_valid_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filestream_is_valid_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_managedstream_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_managedstream_new();
			if((<any>SkiaSharp.ApiOverride).sk_managedstream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedstream_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_managedstream_set_delegates_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_managedstream_set_delegates_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_managedstream_set_delegates_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_managedstream_set_delegates_0_Pre(parms);
			}
			var pRead = parms.pRead;
			var pPeek = parms.pPeek;
			var pIsAtEnd = parms.pIsAtEnd;
			var pHasPosition = parms.pHasPosition;
			var pHasLength = parms.pHasLength;
			var pRewind = parms.pRewind;
			var pGetPosition = parms.pGetPosition;
			var pSeek = parms.pSeek;
			var pMove = parms.pMove;
			var pGetLength = parms.pGetLength;
			var pCreateNew = parms.pCreateNew;
			var pDestroy = parms.pDestroy;
			var ret = CanvasKit._sk_managedstream_set_delegates(pRead, pPeek, pIsAtEnd, pHasPosition, pHasLength, pRewind, pGetPosition, pSeek, pMove, pGetLength, pCreateNew, pDestroy);
			if((<any>SkiaSharp.ApiOverride).sk_managedstream_set_delegates_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedstream_set_delegates_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_managedstream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_managedstream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_managedstream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_managedstream_destroy_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_managedstream_destroy(stream);
			if((<any>SkiaSharp.ApiOverride).sk_managedstream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedstream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_managedwstream_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_managedwstream_new();
			if((<any>SkiaSharp.ApiOverride).sk_managedwstream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedwstream_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_managedwstream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_managedwstream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_managedwstream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_managedwstream_destroy_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_managedwstream_destroy(stream);
			if((<any>SkiaSharp.ApiOverride).sk_managedwstream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedwstream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_managedwstream_set_delegates_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_managedwstream_set_delegates_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_managedwstream_set_delegates_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_managedwstream_set_delegates_0_Pre(parms);
			}
			var pWrite = parms.pWrite;
			var pFlush = parms.pFlush;
			var pBytesWritten = parms.pBytesWritten;
			var pDestroy = parms.pDestroy;
			var ret = CanvasKit._sk_managedwstream_set_delegates(pWrite, pFlush, pBytesWritten, pDestroy);
			if((<any>SkiaSharp.ApiOverride).sk_managedwstream_set_delegates_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_managedwstream_set_delegates_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filewstream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filewstream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filewstream_destroy_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_filewstream_destroy(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filewstream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_dynamicmemorywstream_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_destroy_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_dynamicmemorywstream_destroy(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filewstream_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filewstream_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filewstream_new_0_Pre(parms);
			}
			var utf8path = CanvasKit._malloc(parms.utf8path_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.utf8path_Length; i++)
				{
					CanvasKit.HEAPU8[utf8path + i] = parms.utf8path[i];
				}
			}
			var ret = CanvasKit._sk_filewstream_new(utf8path);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filewstream_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_dynamicmemorywstream_new();
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_detach_as_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_dynamicmemorywstream_detach_as_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_stream_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_dynamicmemorywstream_detach_as_stream(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_detach_as_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_dynamicmemorywstream_detach_as_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_data_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_dynamicmemorywstream_detach_as_data(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_detach_as_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_copy_to_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_dynamicmemorywstream_copy_to_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_copy_to_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_copy_to_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var data = parms.data;
			var ret = CanvasKit._sk_dynamicmemorywstream_copy_to(cstream, data);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_copy_to_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_copy_to_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_dynamicmemorywstream_write_to_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_dynamicmemorywstream_write_to_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_write_to_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_write_to_stream_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var dst = parms.dst;
			var ret = CanvasKit._sk_dynamicmemorywstream_write_to_stream(cstream, dst);
			if((<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_write_to_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_dynamicmemorywstream_write_to_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var buffer = parms.buffer;
			var size = parms.size;
			var ret = CanvasKit._sk_wstream_write(cstream, buffer, size);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_1_Pre(parms);
			}
			var cstream = parms.cstream;
			var buffer = CanvasKit._malloc(parms.buffer_Length * 1); /*byte*/
			
			{
				for(var i = 0; i < parms.buffer_Length; i++)
				{
					CanvasKit.HEAPU8[buffer + i] = parms.buffer[i];
				}
			}
			var size = parms.size;
			var ret = CanvasKit._sk_wstream_write(cstream, buffer, size);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_newline_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_newline_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_newline_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_newline_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_wstream_newline(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_newline_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_newline_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_flush_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_flush_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_flush_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_flush_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_wstream_flush(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_flush_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_flush_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_bytes_written_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_bytes_written_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_bytes_written_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_bytes_written_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_wstream_bytes_written(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_bytes_written_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_bytes_written_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_8_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_8_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_8_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_8_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_8(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_8_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_8_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_16_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_16_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_16_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_16_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_16(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_16_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_16_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_32_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_32_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_32_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_32_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_32(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_32_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_32_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_text_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value_Length = parms.value.length*4+1
			var value = CanvasKit._malloc(value_Length);
			CanvasKit.stringToUTF8(parms.value, value, value_Length);
			var ret = CanvasKit._sk_wstream_write_text(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_dec_as_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_dec_as_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_dec_as_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_dec_as_text_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_dec_as_text(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_dec_as_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_dec_as_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_bigdec_as_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_bigdec_as_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_bigdec_as_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_bigdec_as_text_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var minDigits = parms.minDigits;
			var ret = CanvasKit._sk_wstream_write_bigdec_as_text(cstream, value, minDigits);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_bigdec_as_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_bigdec_as_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_hex_as_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_hex_as_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_hex_as_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_hex_as_text_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var minDigits = parms.minDigits;
			var ret = CanvasKit._sk_wstream_write_hex_as_text(cstream, value, minDigits);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_hex_as_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_hex_as_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_scalar_as_text_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_scalar_as_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_as_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_as_text_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_scalar_as_text(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_as_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_as_text_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_bool_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_bool_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_bool_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_bool_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_bool(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_bool_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_bool_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_scalar_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_scalar_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_scalar(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_scalar_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_packed_uint_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_packed_uint_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_packed_uint_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_packed_uint_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_write_packed_uint(cstream, value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_packed_uint_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_packed_uint_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_write_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_write_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_write_stream_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var input = parms.input;
			var length = parms.length;
			var ret = CanvasKit._sk_wstream_write_stream(cstream, input, length);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_write_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_write_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_wstream_get_size_of_packed_uint_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_wstream_get_size_of_packed_uint_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_get_size_of_packed_uint_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_wstream_get_size_of_packed_uint_0_Pre(parms);
			}
			var value = parms.value;
			var ret = CanvasKit._sk_wstream_get_size_of_packed_uint(value);
			if((<any>SkiaSharp.ApiOverride).sk_wstream_get_size_of_packed_uint_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_wstream_get_size_of_packed_uint_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_filewstream_is_valid_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_filewstream_is_valid_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_is_valid_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_filewstream_is_valid_0_Pre(parms);
			}
			var cstream = parms.cstream;
			var ret = CanvasKit._sk_filewstream_is_valid(cstream);
			if((<any>SkiaSharp.ApiOverride).sk_filewstream_is_valid_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_filewstream_is_valid_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_unref_0_Pre(parms);
			}
			var document = parms.document;
			var ret = CanvasKit._sk_document_unref(document);
			if((<any>SkiaSharp.ApiOverride).sk_document_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_create_pdf_from_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_create_pdf_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_document_create_pdf_from_stream(stream);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_create_pdf_from_stream_with_metadata_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_document_create_pdf_from_stream_with_metadata_0_Return();
			var parms = sk_document_create_pdf_from_stream_with_metadata_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_with_metadata_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_with_metadata_0_Pre(parms);
			}
			var stream = parms.stream;
			var metadata = parms.metadata.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_document_create_pdf_from_stream_with_metadata(stream, metadata);
			var retStruct = new sk_document_create_pdf_from_stream_with_metadata_0_Return();
			retStruct.metadata = SkiaSharp.SKDocumentPdfMetadataInternal.unmarshal(metadata, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_with_metadata_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_create_pdf_from_stream_with_metadata_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_document_create_xps_from_stream_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_create_xps_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_xps_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_create_xps_from_stream_0_Pre(parms);
			}
			var stream = parms.stream;
			var dpi = parms.dpi;
			var ret = CanvasKit._sk_document_create_xps_from_stream(stream, dpi);
			if((<any>SkiaSharp.ApiOverride).sk_document_create_xps_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_create_xps_from_stream_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_begin_page_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_document_begin_page_0_Return();
			var parms = sk_document_begin_page_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_begin_page_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_begin_page_0_Pre(parms);
			}
			var document = parms.document;
			var width = parms.width;
			var height = parms.height;
			var content = parms.content.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_document_begin_page(document, width, height, content);
			var retStruct = new sk_document_begin_page_0_Return();
			retStruct.content = SkiaSharp.SKRect.unmarshal(content, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_document_begin_page_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_begin_page_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_document_begin_page_1(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_begin_page_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_begin_page_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_begin_page_1_Pre(parms);
			}
			var document = parms.document;
			var width = parms.width;
			var height = parms.height;
			var contentZero = parms.contentZero;
			var ret = CanvasKit._sk_document_begin_page(document, width, height, contentZero);
			if((<any>SkiaSharp.ApiOverride).sk_document_begin_page_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_begin_page_1_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_end_page_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_end_page_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_end_page_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_end_page_0_Pre(parms);
			}
			var document = parms.document;
			var ret = CanvasKit._sk_document_end_page(document);
			if((<any>SkiaSharp.ApiOverride).sk_document_end_page_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_end_page_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_close_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_close_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_close_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_close_0_Pre(parms);
			}
			var document = parms.document;
			var ret = CanvasKit._sk_document_close(document);
			if((<any>SkiaSharp.ApiOverride).sk_document_close_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_close_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_document_abort_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_document_abort_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_document_abort_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_document_abort_0_Pre(parms);
			}
			var document = parms.document;
			var ret = CanvasKit._sk_document_abort(document);
			if((<any>SkiaSharp.ApiOverride).sk_document_abort_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_document_abort_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_min_buffered_bytes_needed_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_codec_min_buffered_bytes_needed();
			if((<any>SkiaSharp.ApiOverride).sk_codec_min_buffered_bytes_needed_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_min_buffered_bytes_needed_0_Post(ret);
			}
			return ret;
		}
		public static sk_codec_new_from_stream_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_new_from_stream_0_Return();
			var parms = sk_codec_new_from_stream_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_new_from_stream_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_new_from_stream_0_Pre(parms);
			}
			var stream = parms.stream;
			var result = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_codec_new_from_stream(stream, result);
			var retStruct = new sk_codec_new_from_stream_0_Return();
			retStruct.result = CanvasKit.getValue(result, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_codec_new_from_stream_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_new_from_stream_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_new_from_data_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_new_from_data_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_new_from_data_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_new_from_data_0_Pre(parms);
			}
			var data = parms.data;
			var ret = CanvasKit._sk_codec_new_from_data(data);
			if((<any>SkiaSharp.ApiOverride).sk_codec_new_from_data_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_new_from_data_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_destroy_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_destroy(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_info_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_get_info_0_Return();
			var parms = sk_codec_get_info_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_info_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_info_0_Pre(parms);
			}
			var codec = parms.codec;
			var info = retStruct.info.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_get_info(codec, info);
			var retStruct = new sk_codec_get_info_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_info_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_info_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_get_origin_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_origin_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_origin_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_origin_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_get_origin(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_origin_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_origin_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_scaled_dimensions_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_get_scaled_dimensions_0_Return();
			var parms = sk_codec_get_scaled_dimensions_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scaled_dimensions_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_scaled_dimensions_0_Pre(parms);
			}
			var codec = parms.codec;
			var desiredScale = parms.desiredScale;
			var dimensions = retStruct.dimensions.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_get_scaled_dimensions(codec, desiredScale, dimensions);
			var retStruct = new sk_codec_get_scaled_dimensions_0_Return();
			retStruct.dimensions = SkiaSharp.SKSizeI.unmarshal(dimensions, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scaled_dimensions_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_scaled_dimensions_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_get_valid_subset_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_get_valid_subset_0_Return();
			var parms = sk_codec_get_valid_subset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_valid_subset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_valid_subset_0_Pre(parms);
			}
			var codec = parms.codec;
			var desiredSubset = parms.desiredSubset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_get_valid_subset(codec, desiredSubset);
			var retStruct = new sk_codec_get_valid_subset_0_Return();
			retStruct.desiredSubset = SkiaSharp.SKRectI.unmarshal(desiredSubset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_valid_subset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_valid_subset_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_get_encoded_format_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_encoded_format_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_encoded_format_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_encoded_format_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_get_encoded_format(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_encoded_format_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_encoded_format_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_get_pixels_0_Return();
			var parms = sk_codec_get_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_pixels_0_Pre(parms);
			}
			var codec = parms.codec;
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_get_pixels(codec, info, pixels, rowBytes, options);
			var retStruct = new sk_codec_get_pixels_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			retStruct.options = SkiaSharp.SKCodecOptionsInternal.unmarshal(options, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_start_incremental_decode_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_start_incremental_decode_0_Return();
			var parms = sk_codec_start_incremental_decode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_0_Pre(parms);
			}
			var codec = parms.codec;
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_start_incremental_decode(codec, info, pixels, rowBytes, options);
			var retStruct = new sk_codec_start_incremental_decode_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			retStruct.options = SkiaSharp.SKCodecOptionsInternal.unmarshal(options, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_start_incremental_decode_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_start_incremental_decode_1_Return();
			var parms = sk_codec_start_incremental_decode_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_1_Pre(parms);
			}
			var codec = parms.codec;
			var info = parms.info.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var optionsZero = parms.optionsZero;
			var ret = CanvasKit._sk_codec_start_incremental_decode(codec, info, pixels, rowBytes, optionsZero);
			var retStruct = new sk_codec_start_incremental_decode_1_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_start_incremental_decode_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_incremental_decode_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_incremental_decode_0_Return();
			var parms = sk_codec_incremental_decode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_incremental_decode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_incremental_decode_0_Pre(parms);
			}
			var codec = parms.codec;
			var rowsDecoded = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_codec_incremental_decode(codec, rowsDecoded);
			var retStruct = new sk_codec_incremental_decode_0_Return();
			retStruct.rowsDecoded = CanvasKit.getValue(rowsDecoded, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_codec_incremental_decode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_incremental_decode_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_get_repetition_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_repetition_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_repetition_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_repetition_count_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_get_repetition_count(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_repetition_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_repetition_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_frame_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_frame_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_frame_count_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_get_frame_count(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_frame_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_frame_info_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_frame_info_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_0_Pre(parms);
			}
			var codec = parms.codec;
			var frameInfo = parms.frameInfo;
			var ret = CanvasKit._sk_codec_get_frame_info(codec, frameInfo);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_frame_info_for_index_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_get_frame_info_for_index_0_Return();
			var parms = sk_codec_get_frame_info_for_index_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_for_index_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_for_index_0_Pre(parms);
			}
			var codec = parms.codec;
			var index = parms.index;
			var frameInfo = retStruct.frameInfo.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_get_frame_info_for_index(codec, index, frameInfo);
			var retStruct = new sk_codec_get_frame_info_for_index_0_Return();
			retStruct.frameInfo = SkiaSharp.SKCodecFrameInfo.unmarshal(frameInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_for_index_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_frame_info_for_index_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_start_scanline_decode_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_start_scanline_decode_0_Return();
			var parms = sk_codec_start_scanline_decode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_0_Pre(parms);
			}
			var codec = parms.codec;
			var info = parms.info.marshalNew(CanvasKit);
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_codec_start_scanline_decode(codec, info, options);
			var retStruct = new sk_codec_start_scanline_decode_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			retStruct.options = SkiaSharp.SKCodecOptionsInternal.unmarshal(options, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_start_scanline_decode_1(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_codec_start_scanline_decode_1_Return();
			var parms = sk_codec_start_scanline_decode_1_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_1_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_1_Pre(parms);
			}
			var codec = parms.codec;
			var info = parms.info.marshalNew(CanvasKit);
			var optionsZero = parms.optionsZero;
			var ret = CanvasKit._sk_codec_start_scanline_decode(codec, info, optionsZero);
			var retStruct = new sk_codec_start_scanline_decode_1_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_1_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_start_scanline_decode_1_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_codec_get_scanlines_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_scanlines_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scanlines_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_scanlines_0_Pre(parms);
			}
			var codec = parms.codec;
			var dst = parms.dst;
			var countLines = parms.countLines;
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_codec_get_scanlines(codec, dst, countLines, rowBytes);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scanlines_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_scanlines_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_skip_scanlines_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_skip_scanlines_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_skip_scanlines_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_skip_scanlines_0_Pre(parms);
			}
			var codec = parms.codec;
			var countLines = parms.countLines;
			var ret = CanvasKit._sk_codec_skip_scanlines(codec, countLines);
			if((<any>SkiaSharp.ApiOverride).sk_codec_skip_scanlines_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_skip_scanlines_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_get_scanline_order_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_get_scanline_order_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scanline_order_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_get_scanline_order_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_get_scanline_order(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_get_scanline_order_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_get_scanline_order_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_next_scanline_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_next_scanline_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_next_scanline_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_next_scanline_0_Pre(parms);
			}
			var codec = parms.codec;
			var ret = CanvasKit._sk_codec_next_scanline(codec);
			if((<any>SkiaSharp.ApiOverride).sk_codec_next_scanline_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_next_scanline_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_codec_output_scanline_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_codec_output_scanline_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_codec_output_scanline_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_codec_output_scanline_0_Pre(parms);
			}
			var codec = parms.codec;
			var inputScanline = parms.inputScanline;
			var ret = CanvasKit._sk_codec_output_scanline(codec, inputScanline);
			if((<any>SkiaSharp.ApiOverride).sk_codec_output_scanline_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_codec_output_scanline_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_bitmap_new();
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_bitmap_destructor_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_destructor_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_destructor_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_destructor_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_destructor(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_destructor_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_destructor_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_info_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_get_info_0_Return();
			var parms = sk_bitmap_get_info_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_info_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_info_0_Pre(parms);
			}
			var b = parms.b;
			var info = retStruct.info.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_bitmap_get_info(b, info);
			var retStruct = new sk_bitmap_get_info_0_Return();
			retStruct.info = SkiaSharp.SKImageInfoNative.unmarshal(info, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_info_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_info_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_get_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_get_pixels_0_Return();
			var parms = sk_bitmap_get_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixels_0_Pre(parms);
			}
			var b = parms.b;
			var length = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_bitmap_get_pixels(b, length);
			var retStruct = new sk_bitmap_get_pixels_0_Return();
			retStruct.length = CanvasKit.getValue(length, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_get_pixel_colors_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_pixel_colors_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_colors_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_colors_0_Pre(parms);
			}
			var b = parms.b;
			var colors = parms.colors;
			var ret = CanvasKit._sk_bitmap_get_pixel_colors(b, colors);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_colors_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_colors_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_set_pixel_colors_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_set_pixel_colors_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_colors_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_colors_0_Pre(parms);
			}
			var b = parms.b;
			var colors = parms.colors;
			var ret = CanvasKit._sk_bitmap_set_pixel_colors(b, colors);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_colors_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_colors_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_reset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_reset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_reset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_reset_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_reset(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_reset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_reset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_row_bytes_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_row_bytes_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_row_bytes_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_row_bytes_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_get_row_bytes(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_row_bytes_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_row_bytes_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_byte_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_byte_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_byte_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_byte_count_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_get_byte_count(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_byte_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_byte_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_is_null_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_is_null_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_null_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_is_null_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_is_null(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_null_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_is_null_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_is_immutable_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_is_immutable_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_immutable_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_is_immutable_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_is_immutable(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_immutable_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_is_immutable_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_set_immutable_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_set_immutable_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_immutable_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_set_immutable_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_set_immutable(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_immutable_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_set_immutable_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_is_volatile_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_is_volatile_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_volatile_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_is_volatile_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_is_volatile(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_is_volatile_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_is_volatile_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_set_volatile_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_set_volatile_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_volatile_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_set_volatile_0_Pre(parms);
			}
			var b = parms.b;
			var value = parms.value;
			var ret = CanvasKit._sk_bitmap_set_volatile(b, value);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_volatile_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_set_volatile_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_erase_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_erase_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_erase_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_erase_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var color = parms.color.color;
			var ret = CanvasKit._sk_bitmap_erase(cbitmap, color);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_erase_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_erase_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_erase_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_erase_rect_0_Return();
			var parms = sk_bitmap_erase_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_erase_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_erase_rect_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var color = parms.color.color;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_bitmap_erase_rect(cbitmap, color, rect);
			var retStruct = new sk_bitmap_erase_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRectI.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_erase_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_erase_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_get_addr_8_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_addr_8_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_8_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_8_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_bitmap_get_addr_8(cbitmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_8_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_8_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_addr_16_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_addr_16_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_16_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_16_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_bitmap_get_addr_16(cbitmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_16_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_16_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_addr_32_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_addr_32_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_32_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_32_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_bitmap_get_addr_32(cbitmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_32_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_32_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_addr_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_addr_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_bitmap_get_addr(cbitmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_addr_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_get_pixel_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_get_pixel_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_color_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_bitmap_get_pixel_color(cbitmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_get_pixel_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_set_pixel_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_set_pixel_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_color_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var x = parms.x;
			var y = parms.y;
			var color = parms.color.color;
			var ret = CanvasKit._sk_bitmap_set_pixel_color(cbitmap, x, y, color);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixel_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_ready_to_draw_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_ready_to_draw_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_ready_to_draw_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_ready_to_draw_0_Pre(parms);
			}
			var b = parms.b;
			var ret = CanvasKit._sk_bitmap_ready_to_draw(b);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_ready_to_draw_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_ready_to_draw_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_install_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_install_pixels_0_Return();
			var parms = sk_bitmap_install_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cinfo = parms.cinfo.marshalNew(CanvasKit);
			var pixels = parms.pixels;
			var rowBytes = parms.rowBytes;
			var releaseProc = parms.releaseProc;
			var context = parms.context;
			var ret = CanvasKit._sk_bitmap_install_pixels(cbitmap, cinfo, pixels, rowBytes, releaseProc, context);
			var retStruct = new sk_bitmap_install_pixels_0_Return();
			retStruct.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(cinfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_install_pixels_with_pixmap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_install_pixels_with_pixmap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_with_pixmap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_with_pixmap_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_bitmap_install_pixels_with_pixmap(cbitmap, cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_with_pixmap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_install_pixels_with_pixmap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_install_mask_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_install_mask_pixels_0_Return();
			var parms = sk_bitmap_install_mask_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_mask_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_install_mask_pixels_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_bitmap_install_mask_pixels(cbitmap, cmask);
			var retStruct = new sk_bitmap_install_mask_pixels_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_install_mask_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_install_mask_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_try_alloc_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_try_alloc_pixels_0_Return();
			var parms = sk_bitmap_try_alloc_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var requestedInfo = parms.requestedInfo.marshalNew(CanvasKit);
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_bitmap_try_alloc_pixels(cbitmap, requestedInfo, rowBytes);
			var retStruct = new sk_bitmap_try_alloc_pixels_0_Return();
			retStruct.requestedInfo = SkiaSharp.SKImageInfoNative.unmarshal(requestedInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_try_alloc_pixels_with_flags_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_try_alloc_pixels_with_flags_0_Return();
			var parms = sk_bitmap_try_alloc_pixels_with_flags_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_with_flags_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_with_flags_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var requestedInfo = parms.requestedInfo.marshalNew(CanvasKit);
			var flags = parms.flags;
			var ret = CanvasKit._sk_bitmap_try_alloc_pixels_with_flags(cbitmap, requestedInfo, flags);
			var retStruct = new sk_bitmap_try_alloc_pixels_with_flags_0_Return();
			retStruct.requestedInfo = SkiaSharp.SKImageInfoNative.unmarshal(requestedInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_with_flags_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_try_alloc_pixels_with_flags_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_set_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_set_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixels_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var pixels = parms.pixels;
			var ret = CanvasKit._sk_bitmap_set_pixels(cbitmap, pixels);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_set_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_peek_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_peek_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_peek_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_peek_pixels_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_bitmap_peek_pixels(cbitmap, cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_peek_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_peek_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_extract_subset_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_extract_subset_0_Return();
			var parms = sk_bitmap_extract_subset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_extract_subset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_extract_subset_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cdst = parms.cdst;
			var subset = parms.subset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_bitmap_extract_subset(cbitmap, cdst, subset);
			var retStruct = new sk_bitmap_extract_subset_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_extract_subset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_extract_subset_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_extract_alpha_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_bitmap_extract_alpha_0_Return();
			var parms = sk_bitmap_extract_alpha_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_extract_alpha_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_extract_alpha_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var dst = parms.dst;
			var paint = parms.paint;
			var offset = retStruct.offset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_bitmap_extract_alpha(cbitmap, dst, paint, offset);
			var retStruct = new sk_bitmap_extract_alpha_0_Return();
			retStruct.offset = SkiaSharp.SKPointI.unmarshal(offset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_extract_alpha_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_extract_alpha_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_bitmap_notify_pixels_changed_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_notify_pixels_changed_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_notify_pixels_changed_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_notify_pixels_changed_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var ret = CanvasKit._sk_bitmap_notify_pixels_changed(cbitmap);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_notify_pixels_changed_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_notify_pixels_changed_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_bitmap_swap_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_bitmap_swap_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_swap_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_bitmap_swap_0_Pre(parms);
			}
			var cbitmap = parms.cbitmap;
			var cother = parms.cother;
			var ret = CanvasKit._sk_bitmap_swap(cbitmap, cother);
			if((<any>SkiaSharp.ApiOverride).sk_bitmap_swap_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_bitmap_swap_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_color_unpremultiply_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_color_unpremultiply_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_0_Pre(parms);
			}
			var pmcolor = parms.pmcolor.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_color_unpremultiply(pmcolor);
			if((<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_color_premultiply_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_color_premultiply_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_color_premultiply_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_color_premultiply_0_Pre(parms);
			}
			var color = parms.color.color;
			var ret = CanvasKit._sk_color_premultiply(color);
			if((<any>SkiaSharp.ApiOverride).sk_color_premultiply_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_color_premultiply_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_color_unpremultiply_array_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_color_unpremultiply_array_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_array_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_array_0_Pre(parms);
			}
			var pmcolors = parms.pmcolors;
			var size = parms.size;
			var colors = parms.colors;
			var ret = CanvasKit._sk_color_unpremultiply_array(pmcolors, size, colors);
			if((<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_array_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_color_unpremultiply_array_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_color_premultiply_array_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_color_premultiply_array_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_color_premultiply_array_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_color_premultiply_array_0_Pre(parms);
			}
			var colors = parms.colors;
			var size = parms.size;
			var pmcolors = parms.pmcolors;
			var ret = CanvasKit._sk_color_premultiply_array(colors, size, pmcolors);
			if((<any>SkiaSharp.ApiOverride).sk_color_premultiply_array_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_color_premultiply_array_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_color_get_bit_shift_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_color_get_bit_shift_0_Return();
			var parms = sk_color_get_bit_shift_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_color_get_bit_shift_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_color_get_bit_shift_0_Pre(parms);
			}
			var a = CanvasKit._malloc(4);
			var r = CanvasKit._malloc(4);
			var g = CanvasKit._malloc(4);
			var b = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_color_get_bit_shift(a, r, g, b);
			var retStruct = new sk_color_get_bit_shift_0_Return();
			retStruct.a = CanvasKit.getValue(a, "i32");
			retStruct.r = CanvasKit.getValue(r, "i32");
			retStruct.g = CanvasKit.getValue(g, "i32");
			retStruct.b = CanvasKit.getValue(b, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_color_get_bit_shift_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_color_get_bit_shift_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_destructor_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_destructor_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_destructor_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_destructor_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_pixmap_destructor(cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_destructor_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_destructor_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_pixmap_new();
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_pixmap_new_with_params_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_new_with_params_0_Return();
			var parms = sk_pixmap_new_with_params_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_new_with_params_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_new_with_params_0_Pre(parms);
			}
			var cinfo = parms.cinfo.marshalNew(CanvasKit);
			var addr = parms.addr;
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_pixmap_new_with_params(cinfo, addr, rowBytes);
			var retStruct = new sk_pixmap_new_with_params_0_Return();
			retStruct.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(cinfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_new_with_params_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_new_with_params_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_reset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_reset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_reset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_reset_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_pixmap_reset(cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_reset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_reset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_reset_with_params_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_reset_with_params_0_Return();
			var parms = sk_pixmap_reset_with_params_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_reset_with_params_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_reset_with_params_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var cinfo = parms.cinfo.marshalNew(CanvasKit);
			var addr = parms.addr;
			var rowBytes = parms.rowBytes;
			var ret = CanvasKit._sk_pixmap_reset_with_params(cpixmap, cinfo, addr, rowBytes);
			var retStruct = new sk_pixmap_reset_with_params_0_Return();
			retStruct.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(cinfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_reset_with_params_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_reset_with_params_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_get_info_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_get_info_0_Return();
			var parms = sk_pixmap_get_info_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_info_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_get_info_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var cinfo = retStruct.cinfo.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pixmap_get_info(cpixmap, cinfo);
			var retStruct = new sk_pixmap_get_info_0_Return();
			retStruct.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(cinfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_info_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_get_info_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_get_row_bytes_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_get_row_bytes_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_row_bytes_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_get_row_bytes_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_pixmap_get_row_bytes(cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_row_bytes_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_get_row_bytes_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_get_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_get_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var ret = CanvasKit._sk_pixmap_get_pixels(cpixmap);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_get_pixels_with_xy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_get_pixels_with_xy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_with_xy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_with_xy_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_pixmap_get_pixels_with_xy(cpixmap, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_with_xy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixels_with_xy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_get_pixel_color_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_get_pixel_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixel_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixel_color_0_Pre(parms);
			}
			var t = parms.t;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_pixmap_get_pixel_color(t, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixel_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_get_pixel_color_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_extract_subset_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_extract_subset_0_Return();
			var parms = sk_pixmap_extract_subset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_extract_subset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_extract_subset_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var result = parms.result;
			var subset = parms.subset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pixmap_extract_subset(cpixmap, result, subset);
			var retStruct = new sk_pixmap_extract_subset_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_extract_subset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_extract_subset_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_erase_color_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_erase_color_0_Return();
			var parms = sk_pixmap_erase_color_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_erase_color_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_erase_color_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var color = parms.color.color;
			var subset = parms.subset.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pixmap_erase_color(cpixmap, color, subset);
			var retStruct = new sk_pixmap_erase_color_0_Return();
			retStruct.subset = SkiaSharp.SKRectI.unmarshal(subset, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_erase_color_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_erase_color_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_encode_image_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_encode_image_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_encode_image_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_encode_image_0_Pre(parms);
			}
			var dst = parms.dst;
			var src = parms.src;
			var encoder = parms.encoder;
			var quality = parms.quality;
			var ret = CanvasKit._sk_pixmap_encode_image(dst, src, encoder, quality);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_encode_image_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_encode_image_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pixmap_read_pixels_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_pixmap_read_pixels_0_Return();
			var parms = sk_pixmap_read_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_read_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_read_pixels_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var dstInfo = parms.dstInfo.marshalNew(CanvasKit);
			var dstPixels = parms.dstPixels;
			var dstRowBytes = parms.dstRowBytes;
			var srcX = parms.srcX;
			var srcY = parms.srcY;
			var behavior = parms.behavior;
			var ret = CanvasKit._sk_pixmap_read_pixels(cpixmap, dstInfo, dstPixels, dstRowBytes, srcX, srcY, behavior);
			var retStruct = new sk_pixmap_read_pixels_0_Return();
			retStruct.dstInfo = SkiaSharp.SKImageInfoNative.unmarshal(dstInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_read_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_read_pixels_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_pixmap_scale_pixels_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pixmap_scale_pixels_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_scale_pixels_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pixmap_scale_pixels_0_Pre(parms);
			}
			var cpixmap = parms.cpixmap;
			var dst = parms.dst;
			var quality = parms.quality;
			var ret = CanvasKit._sk_pixmap_scale_pixels(cpixmap, dst, quality);
			if((<any>SkiaSharp.ApiOverride).sk_pixmap_scale_pixels_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pixmap_scale_pixels_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_swizzle_swap_rb_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_swizzle_swap_rb_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_swizzle_swap_rb_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_swizzle_swap_rb_0_Pre(parms);
			}
			var dest = parms.dest;
			var src = parms.src;
			var count = parms.count;
			var ret = CanvasKit._sk_swizzle_swap_rb(dest, src, count);
			if((<any>SkiaSharp.ApiOverride).sk_swizzle_swap_rb_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_swizzle_swap_rb_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_webpencoder_encode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_webpencoder_encode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_webpencoder_encode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_webpencoder_encode_0_Pre(parms);
			}
			var dst = parms.dst;
			var src = parms.src;
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_webpencoder_encode(dst, src, options);
			if((<any>SkiaSharp.ApiOverride).sk_webpencoder_encode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_webpencoder_encode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_jpegencoder_encode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_jpegencoder_encode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_jpegencoder_encode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_jpegencoder_encode_0_Pre(parms);
			}
			var dst = parms.dst;
			var src = parms.src;
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_jpegencoder_encode(dst, src, options);
			if((<any>SkiaSharp.ApiOverride).sk_jpegencoder_encode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_jpegencoder_encode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_pngencoder_encode_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_pngencoder_encode_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_pngencoder_encode_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_pngencoder_encode_0_Pre(parms);
			}
			var dst = parms.dst;
			var src = parms.src;
			var options = parms.options.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_pngencoder_encode(dst, src, options);
			if((<any>SkiaSharp.ApiOverride).sk_pngencoder_encode_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_pngencoder_encode_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_mask_alloc_image_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_mask_alloc_image_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_alloc_image_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_alloc_image_0_Pre(parms);
			}
			var bytes = parms.bytes;
			var ret = CanvasKit._sk_mask_alloc_image(bytes);
			if((<any>SkiaSharp.ApiOverride).sk_mask_alloc_image_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_alloc_image_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_mask_free_image_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_mask_free_image_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_free_image_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_free_image_0_Pre(parms);
			}
			var image = parms.image;
			var ret = CanvasKit._sk_mask_free_image(image);
			if((<any>SkiaSharp.ApiOverride).sk_mask_free_image_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_free_image_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_mask_is_empty_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_is_empty_0_Return();
			var parms = sk_mask_is_empty_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_is_empty_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_is_empty_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_mask_is_empty(cmask);
			var retStruct = new sk_mask_is_empty_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_is_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_is_empty_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_compute_image_size_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_compute_image_size_0_Return();
			var parms = sk_mask_compute_image_size_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_compute_image_size_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_compute_image_size_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_mask_compute_image_size(cmask);
			var retStruct = new sk_mask_compute_image_size_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_compute_image_size_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_compute_image_size_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_compute_total_image_size_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_compute_total_image_size_0_Return();
			var parms = sk_mask_compute_total_image_size_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_compute_total_image_size_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_compute_total_image_size_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_mask_compute_total_image_size(cmask);
			var retStruct = new sk_mask_compute_total_image_size_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_compute_total_image_size_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_compute_total_image_size_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_get_addr_1_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_get_addr_1_0_Return();
			var parms = sk_mask_get_addr_1_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_1_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_get_addr_1_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_mask_get_addr_1(cmask, x, y);
			var retStruct = new sk_mask_get_addr_1_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_1_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_get_addr_1_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_get_addr_8_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_get_addr_8_0_Return();
			var parms = sk_mask_get_addr_8_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_8_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_get_addr_8_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_mask_get_addr_8(cmask, x, y);
			var retStruct = new sk_mask_get_addr_8_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_8_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_get_addr_8_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_get_addr_lcd_16_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_get_addr_lcd_16_0_Return();
			var parms = sk_mask_get_addr_lcd_16_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_lcd_16_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_get_addr_lcd_16_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_mask_get_addr_lcd_16(cmask, x, y);
			var retStruct = new sk_mask_get_addr_lcd_16_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_lcd_16_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_get_addr_lcd_16_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_get_addr_32_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_get_addr_32_0_Return();
			var parms = sk_mask_get_addr_32_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_32_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_get_addr_32_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_mask_get_addr_32(cmask, x, y);
			var retStruct = new sk_mask_get_addr_32_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_32_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_get_addr_32_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_mask_get_addr_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_mask_get_addr_0_Return();
			var parms = sk_mask_get_addr_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_mask_get_addr_0_Pre(parms);
			}
			var cmask = parms.cmask.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_mask_get_addr(cmask, x, y);
			var retStruct = new sk_mask_get_addr_0_Return();
			retStruct.cmask = SkiaSharp.SKMask.unmarshal(cmask, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_mask_get_addr_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_mask_get_addr_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_try_invert_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_try_invert_0_Return();
			var parms = sk_matrix_try_invert_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_try_invert_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_try_invert_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var result = retStruct.result.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_try_invert(matrix, result);
			var retStruct = new sk_matrix_try_invert_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			retStruct.result = SkiaSharp.SKMatrix.unmarshal(result, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_try_invert_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_try_invert_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_concat_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_concat_0_Return();
			var parms = sk_matrix_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_concat_0_Pre(parms);
			}
			var target = parms.target.marshalNew(CanvasKit);
			var first = parms.first.marshalNew(CanvasKit);
			var second = parms.second.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_concat(target, first, second);
			var retStruct = new sk_matrix_concat_0_Return();
			retStruct.target = SkiaSharp.SKMatrix.unmarshal(target, CanvasKit);
			retStruct.first = SkiaSharp.SKMatrix.unmarshal(first, CanvasKit);
			retStruct.second = SkiaSharp.SKMatrix.unmarshal(second, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_concat_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_pre_concat_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_pre_concat_0_Return();
			var parms = sk_matrix_pre_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_pre_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_pre_concat_0_Pre(parms);
			}
			var target = parms.target.marshalNew(CanvasKit);
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_pre_concat(target, matrix);
			var retStruct = new sk_matrix_pre_concat_0_Return();
			retStruct.target = SkiaSharp.SKMatrix.unmarshal(target, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_pre_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_pre_concat_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_post_concat_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_post_concat_0_Return();
			var parms = sk_matrix_post_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_post_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_post_concat_0_Pre(parms);
			}
			var target = parms.target.marshalNew(CanvasKit);
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_post_concat(target, matrix);
			var retStruct = new sk_matrix_post_concat_0_Return();
			retStruct.target = SkiaSharp.SKMatrix.unmarshal(target, CanvasKit);
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_post_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_post_concat_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_rect_0_Return();
			var parms = sk_matrix_map_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_rect_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var dest = retStruct.dest.marshalNew(CanvasKit);
			var source = parms.source.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_map_rect(matrix, dest, source);
			var retStruct = new sk_matrix_map_rect_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			retStruct.dest = SkiaSharp.SKRect.unmarshal(dest, CanvasKit);
			retStruct.source = SkiaSharp.SKRect.unmarshal(source, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_points_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_points_0_Return();
			var parms = sk_matrix_map_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_points_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var dst = parms.dst;
			var src = parms.src;
			var count = parms.count;
			var ret = CanvasKit._sk_matrix_map_points(matrix, dst, src, count);
			var retStruct = new sk_matrix_map_points_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_points_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_vectors_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_vectors_0_Return();
			var parms = sk_matrix_map_vectors_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_vectors_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_vectors_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var dst = parms.dst;
			var src = parms.src;
			var count = parms.count;
			var ret = CanvasKit._sk_matrix_map_vectors(matrix, dst, src, count);
			var retStruct = new sk_matrix_map_vectors_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_vectors_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_vectors_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_xy_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_xy_0_Return();
			var parms = sk_matrix_map_xy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_xy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_xy_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var result = retStruct.result.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_map_xy(matrix, x, y, result);
			var retStruct = new sk_matrix_map_xy_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			retStruct.result = SkiaSharp.SKPoint.unmarshal(result, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_xy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_xy_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_vector_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_vector_0_Return();
			var parms = sk_matrix_map_vector_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_vector_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_vector_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var x = parms.x;
			var y = parms.y;
			var result = retStruct.result.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix_map_vector(matrix, x, y, result);
			var retStruct = new sk_matrix_map_vector_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			retStruct.result = SkiaSharp.SKPoint.unmarshal(result, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_vector_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_vector_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix_map_radius_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix_map_radius_0_Return();
			var parms = sk_matrix_map_radius_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_radius_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix_map_radius_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var radius = parms.radius;
			var ret = CanvasKit._sk_matrix_map_radius(matrix, radius);
			var retStruct = new sk_matrix_map_radius_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix_map_radius_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix_map_radius_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_3dview_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_3dview_new();
			if((<any>SkiaSharp.ApiOverride).sk_3dview_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_3dview_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_destroy_0_Pre(parms);
			}
			var cview = parms.cview;
			var ret = CanvasKit._sk_3dview_destroy(cview);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_save_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_save_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_save_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_save_0_Pre(parms);
			}
			var cview = parms.cview;
			var ret = CanvasKit._sk_3dview_save(cview);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_save_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_save_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_restore_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_restore_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_restore_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_restore_0_Pre(parms);
			}
			var cview = parms.cview;
			var ret = CanvasKit._sk_3dview_restore(cview);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_restore_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_restore_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_translate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_translate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_translate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_translate_0_Pre(parms);
			}
			var cview = parms.cview;
			var x = parms.x;
			var y = parms.y;
			var z = parms.z;
			var ret = CanvasKit._sk_3dview_translate(cview, x, y, z);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_translate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_translate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_x_degrees_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_x_degrees_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_degrees_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_degrees_0_Pre(parms);
			}
			var cview = parms.cview;
			var degrees = parms.degrees;
			var ret = CanvasKit._sk_3dview_rotate_x_degrees(cview, degrees);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_degrees_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_degrees_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_y_degrees_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_y_degrees_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_degrees_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_degrees_0_Pre(parms);
			}
			var cview = parms.cview;
			var degrees = parms.degrees;
			var ret = CanvasKit._sk_3dview_rotate_y_degrees(cview, degrees);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_degrees_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_degrees_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_z_degrees_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_z_degrees_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_degrees_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_degrees_0_Pre(parms);
			}
			var cview = parms.cview;
			var degrees = parms.degrees;
			var ret = CanvasKit._sk_3dview_rotate_z_degrees(cview, degrees);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_degrees_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_degrees_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_x_radians_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_x_radians_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_radians_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_radians_0_Pre(parms);
			}
			var cview = parms.cview;
			var radians = parms.radians;
			var ret = CanvasKit._sk_3dview_rotate_x_radians(cview, radians);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_radians_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_x_radians_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_y_radians_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_y_radians_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_radians_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_radians_0_Pre(parms);
			}
			var cview = parms.cview;
			var radians = parms.radians;
			var ret = CanvasKit._sk_3dview_rotate_y_radians(cview, radians);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_radians_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_y_radians_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_rotate_z_radians_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_rotate_z_radians_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_radians_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_radians_0_Pre(parms);
			}
			var cview = parms.cview;
			var radians = parms.radians;
			var ret = CanvasKit._sk_3dview_rotate_z_radians(cview, radians);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_radians_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_rotate_z_radians_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_get_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_3dview_get_matrix_0_Return();
			var parms = sk_3dview_get_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_get_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_get_matrix_0_Pre(parms);
			}
			var cview = parms.cview;
			var cmatrix = parms.cmatrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_3dview_get_matrix(cview, cmatrix);
			var retStruct = new sk_3dview_get_matrix_0_Return();
			retStruct.cmatrix = SkiaSharp.SKMatrix.unmarshal(cmatrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_get_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_get_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_3dview_apply_to_canvas_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_apply_to_canvas_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_apply_to_canvas_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_apply_to_canvas_0_Pre(parms);
			}
			var cview = parms.cview;
			var ccanvas = parms.ccanvas;
			var ret = CanvasKit._sk_3dview_apply_to_canvas(cview, ccanvas);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_apply_to_canvas_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_apply_to_canvas_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_3dview_dot_with_normal_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_3dview_dot_with_normal_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_dot_with_normal_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_3dview_dot_with_normal_0_Pre(parms);
			}
			var cview = parms.cview;
			var dx = parms.dx;
			var dy = parms.dy;
			var dz = parms.dz;
			var ret = CanvasKit._sk_3dview_dot_with_normal(cview, dx, dy, dz);
			if((<any>SkiaSharp.ApiOverride).sk_3dview_dot_with_normal_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_3dview_dot_with_normal_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_destroy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_destroy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_destroy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_destroy_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var ret = CanvasKit._sk_matrix44_destroy(matrix);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_destroy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_destroy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_matrix44_new();
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_matrix44_new_identity_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_matrix44_new_identity();
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_identity_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_new_identity_0_Post(ret);
			}
			return ret;
		}
		public static sk_matrix44_new_copy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_new_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_new_copy_0_Pre(parms);
			}
			var src = parms.src;
			var ret = CanvasKit._sk_matrix44_new_copy(src);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_new_copy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_new_concat_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_new_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_new_concat_0_Pre(parms);
			}
			var a = parms.a;
			var b = parms.b;
			var ret = CanvasKit._sk_matrix44_new_concat(a, b);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_new_concat_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_new_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix44_new_matrix_0_Return();
			var parms = sk_matrix44_new_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_new_matrix_0_Pre(parms);
			}
			var src = parms.src.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix44_new_matrix(src);
			var retStruct = new sk_matrix44_new_matrix_0_Return();
			retStruct.src = SkiaSharp.SKMatrix.unmarshal(src, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_new_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_new_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix44_equals_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_equals_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_equals_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_equals_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var other = parms.other;
			var ret = CanvasKit._sk_matrix44_equals(matrix, other);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_equals_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_equals_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_to_matrix_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_matrix44_to_matrix_0_Return();
			var parms = sk_matrix44_to_matrix_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_to_matrix_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_to_matrix_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dst = retStruct.dst.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_matrix44_to_matrix(matrix, dst);
			var retStruct = new sk_matrix44_to_matrix_0_Return();
			retStruct.dst = SkiaSharp.SKMatrix.unmarshal(dst, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_to_matrix_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_to_matrix_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_matrix44_get_type_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_get_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_get_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_get_type_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var ret = CanvasKit._sk_matrix44_get_type(matrix);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_get_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_get_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_identity_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_identity_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_identity_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_identity_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var ret = CanvasKit._sk_matrix44_set_identity(matrix);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_identity_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_identity_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_get_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_get_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_get_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_get_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var row = parms.row;
			var col = parms.col;
			var ret = CanvasKit._sk_matrix44_get(matrix, row, col);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_get_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_get_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var row = parms.row;
			var col = parms.col;
			var value = parms.value;
			var ret = CanvasKit._sk_matrix44_set(matrix, row, col, value);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_as_col_major_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_as_col_major_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_as_col_major_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_as_col_major_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dst = CanvasKit._malloc(parms.dst_Length * 4); /*float*/
			var dst_f32 = dst / 4;
			
			{
				for(var i = 0; i < parms.dst_Length; i++)
				{
					CanvasKit.HEAPF32[dst_f32 + i] = parms.dst[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_as_col_major(matrix, dst);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_as_col_major_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_as_col_major_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_as_row_major_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_as_row_major_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_as_row_major_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_as_row_major_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dst = CanvasKit._malloc(parms.dst_Length * 4); /*float*/
			var dst_f32 = dst / 4;
			
			{
				for(var i = 0; i < parms.dst_Length; i++)
				{
					CanvasKit.HEAPF32[dst_f32 + i] = parms.dst[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_as_row_major(matrix, dst);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_as_row_major_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_as_row_major_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_col_major_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_col_major_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_col_major_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_col_major_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var src = CanvasKit._malloc(parms.src_Length * 4); /*float*/
			var src_f32 = src / 4;
			
			{
				for(var i = 0; i < parms.src_Length; i++)
				{
					CanvasKit.HEAPF32[src_f32 + i] = parms.src[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_set_col_major(matrix, src);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_col_major_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_col_major_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_row_major_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_row_major_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_row_major_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_row_major_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var src = CanvasKit._malloc(parms.src_Length * 4); /*float*/
			var src_f32 = src / 4;
			
			{
				for(var i = 0; i < parms.src_Length; i++)
				{
					CanvasKit.HEAPF32[src_f32 + i] = parms.src[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_set_row_major(matrix, src);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_row_major_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_row_major_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_translate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_translate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_translate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_translate_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dx = parms.dx;
			var dy = parms.dy;
			var dz = parms.dz;
			var ret = CanvasKit._sk_matrix44_set_translate(matrix, dx, dy, dz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_translate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_translate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_pre_translate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_pre_translate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_translate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_pre_translate_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dx = parms.dx;
			var dy = parms.dy;
			var dz = parms.dz;
			var ret = CanvasKit._sk_matrix44_pre_translate(matrix, dx, dy, dz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_translate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_pre_translate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_post_translate_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_post_translate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_translate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_post_translate_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var dx = parms.dx;
			var dy = parms.dy;
			var dz = parms.dz;
			var ret = CanvasKit._sk_matrix44_post_translate(matrix, dx, dy, dz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_translate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_post_translate_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_scale_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_scale_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_scale_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_scale_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var sx = parms.sx;
			var sy = parms.sy;
			var sz = parms.sz;
			var ret = CanvasKit._sk_matrix44_set_scale(matrix, sx, sy, sz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_scale_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_scale_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_pre_scale_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_pre_scale_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_scale_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_pre_scale_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var sx = parms.sx;
			var sy = parms.sy;
			var sz = parms.sz;
			var ret = CanvasKit._sk_matrix44_pre_scale(matrix, sx, sy, sz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_scale_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_pre_scale_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_post_scale_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_post_scale_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_scale_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_post_scale_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var sx = parms.sx;
			var sy = parms.sy;
			var sz = parms.sz;
			var ret = CanvasKit._sk_matrix44_post_scale(matrix, sx, sy, sz);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_scale_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_post_scale_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_rotate_about_degrees_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_rotate_about_degrees_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_degrees_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_degrees_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var x = parms.x;
			var y = parms.y;
			var z = parms.z;
			var degrees = parms.degrees;
			var ret = CanvasKit._sk_matrix44_set_rotate_about_degrees(matrix, x, y, z, degrees);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_degrees_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_degrees_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_rotate_about_radians_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_rotate_about_radians_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var x = parms.x;
			var y = parms.y;
			var z = parms.z;
			var radians = parms.radians;
			var ret = CanvasKit._sk_matrix44_set_rotate_about_radians(matrix, x, y, z, radians);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_rotate_about_radians_unit_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_rotate_about_radians_unit_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_unit_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_unit_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var x = parms.x;
			var y = parms.y;
			var z = parms.z;
			var radians = parms.radians;
			var ret = CanvasKit._sk_matrix44_set_rotate_about_radians_unit(matrix, x, y, z, radians);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_unit_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_rotate_about_radians_unit_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_set_concat_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_set_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_set_concat_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var a = parms.a;
			var b = parms.b;
			var ret = CanvasKit._sk_matrix44_set_concat(matrix, a, b);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_set_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_set_concat_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_pre_concat_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_pre_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_pre_concat_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var m = parms.m;
			var ret = CanvasKit._sk_matrix44_pre_concat(matrix, m);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_pre_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_pre_concat_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_post_concat_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_post_concat_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_concat_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_post_concat_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var m = parms.m;
			var ret = CanvasKit._sk_matrix44_post_concat(matrix, m);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_post_concat_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_post_concat_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_invert_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_invert_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_invert_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_invert_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var inverse = parms.inverse;
			var ret = CanvasKit._sk_matrix44_invert(matrix, inverse);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_invert_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_invert_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_transpose_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_transpose_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_transpose_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_transpose_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var ret = CanvasKit._sk_matrix44_transpose(matrix);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_transpose_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_transpose_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_map_scalars_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_map_scalars_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_map_scalars_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_map_scalars_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var src = CanvasKit._malloc(parms.src_Length * 4); /*float*/
			var src_f32 = src / 4;
			
			{
				for(var i = 0; i < parms.src_Length; i++)
				{
					CanvasKit.HEAPF32[src_f32 + i] = parms.src[i];
				}
			}
			var dst = CanvasKit._malloc(parms.dst_Length * 4); /*float*/
			var dst_f32 = dst / 4;
			
			{
				for(var i = 0; i < parms.dst_Length; i++)
				{
					CanvasKit.HEAPF32[dst_f32 + i] = parms.dst[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_map_scalars(matrix, src, dst);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_map_scalars_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_map_scalars_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_map2_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_map2_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_map2_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_map2_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var src2 = CanvasKit._malloc(parms.src2_Length * 4); /*float*/
			var src2_f32 = src2 / 4;
			
			{
				for(var i = 0; i < parms.src2_Length; i++)
				{
					CanvasKit.HEAPF32[src2_f32 + i] = parms.src2[i];
				}
			}
			var count = parms.count;
			var dst = CanvasKit._malloc(parms.dst_Length * 4); /*float*/
			var dst_f32 = dst / 4;
			
			{
				for(var i = 0; i < parms.dst_Length; i++)
				{
					CanvasKit.HEAPF32[dst_f32 + i] = parms.dst[i];
				}
			}
			var ret = CanvasKit._sk_matrix44_map2(matrix, src2, count, dst);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_map2_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_map2_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_preserves_2d_axis_alignment_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_preserves_2d_axis_alignment_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_preserves_2d_axis_alignment_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_preserves_2d_axis_alignment_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var epsilon = parms.epsilon;
			var ret = CanvasKit._sk_matrix44_preserves_2d_axis_alignment(matrix, epsilon);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_preserves_2d_axis_alignment_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_preserves_2d_axis_alignment_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_matrix44_determinant_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_matrix44_determinant_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_determinant_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_matrix44_determinant_0_Pre(parms);
			}
			var matrix = parms.matrix;
			var ret = CanvasKit._sk_matrix44_determinant(matrix);
			if((<any>SkiaSharp.ApiOverride).sk_matrix44_determinant_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_matrix44_determinant_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_unref_0_Pre(parms);
			}
			var effect = parms.effect;
			var ret = CanvasKit._sk_path_effect_unref(effect);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_compose_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_compose_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_compose_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_compose_0_Pre(parms);
			}
			var outer = parms.outer;
			var inner = parms.inner;
			var ret = CanvasKit._sk_path_effect_create_compose(outer, inner);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_compose_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_compose_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_sum_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_sum_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_sum_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_sum_0_Pre(parms);
			}
			var first = parms.first;
			var second = parms.second;
			var ret = CanvasKit._sk_path_effect_create_sum(first, second);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_sum_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_sum_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_discrete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_discrete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_discrete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_discrete_0_Pre(parms);
			}
			var segLength = parms.segLength;
			var deviation = parms.deviation;
			var seedAssist = parms.seedAssist;
			var ret = CanvasKit._sk_path_effect_create_discrete(segLength, deviation, seedAssist);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_discrete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_discrete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_corner_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_corner_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_corner_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_corner_0_Pre(parms);
			}
			var radius = parms.radius;
			var ret = CanvasKit._sk_path_effect_create_corner(radius);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_corner_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_corner_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_1d_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_1d_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_1d_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_1d_path_0_Pre(parms);
			}
			var path = parms.path;
			var advance = parms.advance;
			var phase = parms.phase;
			var style = parms.style;
			var ret = CanvasKit._sk_path_effect_create_1d_path(path, advance, phase, style);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_1d_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_1d_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_2d_line_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_effect_create_2d_line_0_Return();
			var parms = sk_path_effect_create_2d_line_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_line_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_line_0_Pre(parms);
			}
			var width = parms.width;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_path_effect_create_2d_line(width, matrix);
			var retStruct = new sk_path_effect_create_2d_line_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_line_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_line_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_effect_create_2d_path_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_path_effect_create_2d_path_0_Return();
			var parms = sk_path_effect_create_2d_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_path_0_Pre(parms);
			}
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var path = parms.path;
			var ret = CanvasKit._sk_path_effect_create_2d_path(matrix, path);
			var retStruct = new sk_path_effect_create_2d_path_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_2d_path_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_path_effect_create_dash_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_dash_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_dash_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_dash_0_Pre(parms);
			}
			var intervals = CanvasKit._malloc(parms.intervals_Length * 4); /*float*/
			var intervals_f32 = intervals / 4;
			
			{
				for(var i = 0; i < parms.intervals_Length; i++)
				{
					CanvasKit.HEAPF32[intervals_f32 + i] = parms.intervals[i];
				}
			}
			var count = parms.count;
			var phase = parms.phase;
			var ret = CanvasKit._sk_path_effect_create_dash(intervals, count, phase);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_dash_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_dash_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_path_effect_create_trim_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_path_effect_create_trim_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_trim_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_path_effect_create_trim_0_Pre(parms);
			}
			var start = parms.start;
			var stop = parms.stop;
			var mode = parms.mode;
			var ret = CanvasKit._sk_path_effect_create_trim(start, stop, mode);
			if((<any>SkiaSharp.ApiOverride).sk_path_effect_create_trim_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_path_effect_create_trim_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colortable_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colortable_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colortable_unref_0_Pre(parms);
			}
			var ctable = parms.ctable;
			var ret = CanvasKit._sk_colortable_unref(ctable);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colortable_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colortable_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colortable_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colortable_new_0_Pre(parms);
			}
			var colors = parms.colors;
			var count = parms.count;
			var ret = CanvasKit._sk_colortable_new(colors, count);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colortable_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colortable_count_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_colortable_count_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_count_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colortable_count_0_Pre(parms);
			}
			var ctable = parms.ctable;
			var ret = CanvasKit._sk_colortable_count(ctable);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_count_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colortable_count_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_colortable_read_colors_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_colortable_read_colors_0_Return();
			var parms = sk_colortable_read_colors_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_colortable_read_colors_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_colortable_read_colors_0_Pre(parms);
			}
			var ctable = parms.ctable;
			var colors = CanvasKit._malloc(4);
			var ret = CanvasKit._sk_colortable_read_colors(ctable, colors);
			var retStruct = new sk_colortable_read_colors_0_Return();
			retStruct.colors = CanvasKit.getValue(colors, "i32");
			if((<any>SkiaSharp.ApiOverride).sk_colortable_read_colors_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_colortable_read_colors_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_context_make_gl_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_make_gl_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_make_gl_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_make_gl_0_Pre(parms);
			}
			var glInterface = parms.glInterface;
			var ret = CanvasKit._gr_context_make_gl(glInterface);
			if((<any>SkiaSharp.ApiOverride).gr_context_make_gl_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_make_gl_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_unref_0_Pre(parms);
			}
			var context = parms.context;
			var ret = CanvasKit._gr_context_unref(context);
			if((<any>SkiaSharp.ApiOverride).gr_context_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_abandon_context_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_abandon_context_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_abandon_context_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_abandon_context_0_Pre(parms);
			}
			var context = parms.context;
			var ret = CanvasKit._gr_context_abandon_context(context);
			if((<any>SkiaSharp.ApiOverride).gr_context_abandon_context_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_abandon_context_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_release_resources_and_abandon_context_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_release_resources_and_abandon_context_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_release_resources_and_abandon_context_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_release_resources_and_abandon_context_0_Pre(parms);
			}
			var context = parms.context;
			var ret = CanvasKit._gr_context_release_resources_and_abandon_context(context);
			if((<any>SkiaSharp.ApiOverride).gr_context_release_resources_and_abandon_context_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_release_resources_and_abandon_context_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_get_resource_cache_limits_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_context_get_resource_cache_limits_0_Return();
			var parms = gr_context_get_resource_cache_limits_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_limits_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_limits_0_Pre(parms);
			}
			var context = parms.context;
			var maxResources = CanvasKit._malloc(4);
			var maxResourceBytes = CanvasKit._malloc(4);
			var ret = CanvasKit._gr_context_get_resource_cache_limits(context, maxResources, maxResourceBytes);
			var retStruct = new gr_context_get_resource_cache_limits_0_Return();
			retStruct.maxResources = CanvasKit.getValue(maxResources, "i32");
			retStruct.maxResourceBytes = CanvasKit.getValue(maxResourceBytes, "i32");
			if((<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_limits_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_limits_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_context_set_resource_cache_limits_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_set_resource_cache_limits_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_set_resource_cache_limits_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_set_resource_cache_limits_0_Pre(parms);
			}
			var context = parms.context;
			var maxResources = parms.maxResources;
			var maxResourceBytes = parms.maxResourceBytes;
			var ret = CanvasKit._gr_context_set_resource_cache_limits(context, maxResources, maxResourceBytes);
			if((<any>SkiaSharp.ApiOverride).gr_context_set_resource_cache_limits_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_set_resource_cache_limits_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_get_resource_cache_usage_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_context_get_resource_cache_usage_0_Return();
			var parms = gr_context_get_resource_cache_usage_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_usage_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_usage_0_Pre(parms);
			}
			var context = parms.context;
			var maxResources = CanvasKit._malloc(4);
			var maxResourceBytes = CanvasKit._malloc(4);
			var ret = CanvasKit._gr_context_get_resource_cache_usage(context, maxResources, maxResourceBytes);
			var retStruct = new gr_context_get_resource_cache_usage_0_Return();
			retStruct.maxResources = CanvasKit.getValue(maxResources, "i32");
			retStruct.maxResourceBytes = CanvasKit.getValue(maxResourceBytes, "i32");
			if((<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_usage_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_get_resource_cache_usage_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_context_get_max_surface_sample_count_for_color_type_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_get_max_surface_sample_count_for_color_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_max_surface_sample_count_for_color_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_get_max_surface_sample_count_for_color_type_0_Pre(parms);
			}
			var context = parms.context;
			var colorType = parms.colorType;
			var ret = CanvasKit._gr_context_get_max_surface_sample_count_for_color_type(context, colorType);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_max_surface_sample_count_for_color_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_get_max_surface_sample_count_for_color_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_flush_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_flush_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_flush_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_flush_0_Pre(parms);
			}
			var context = parms.context;
			var ret = CanvasKit._gr_context_flush(context);
			if((<any>SkiaSharp.ApiOverride).gr_context_flush_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_flush_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_reset_context_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_reset_context_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_reset_context_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_reset_context_0_Pre(parms);
			}
			var context = parms.context;
			var state = parms.state;
			var ret = CanvasKit._gr_context_reset_context(context, state);
			if((<any>SkiaSharp.ApiOverride).gr_context_reset_context_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_reset_context_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_context_get_backend_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_context_get_backend_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_backend_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_context_get_backend_0_Pre(parms);
			}
			var context = parms.context;
			var ret = CanvasKit._gr_context_get_backend(context);
			if((<any>SkiaSharp.ApiOverride).gr_context_get_backend_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_context_get_backend_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_assemble_interface_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_assemble_interface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_interface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_interface_0_Pre(parms);
			}
			var ctx = parms.ctx;
			var get = parms.get;
			var ret = CanvasKit._gr_glinterface_assemble_interface(ctx, get);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_interface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_interface_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_assemble_gl_interface_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_assemble_gl_interface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gl_interface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gl_interface_0_Pre(parms);
			}
			var ctx = parms.ctx;
			var get = parms.get;
			var ret = CanvasKit._gr_glinterface_assemble_gl_interface(ctx, get);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gl_interface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gl_interface_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_assemble_gles_interface_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_assemble_gles_interface_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gles_interface_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gles_interface_0_Pre(parms);
			}
			var ctx = parms.ctx;
			var get = parms.get;
			var ret = CanvasKit._gr_glinterface_assemble_gles_interface(ctx, get);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gles_interface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_assemble_gles_interface_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_create_native_interface_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._gr_glinterface_create_native_interface();
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_create_native_interface_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_create_native_interface_0_Post(ret);
			}
			return ret;
		}
		public static gr_glinterface_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_unref_0_Pre(parms);
			}
			var glInterface = parms.glInterface;
			var ret = CanvasKit._gr_glinterface_unref(glInterface);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_validate_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_validate_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_validate_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_validate_0_Pre(parms);
			}
			var glInterface = parms.glInterface;
			var ret = CanvasKit._gr_glinterface_validate(glInterface);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_validate_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_validate_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_glinterface_has_extension_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_glinterface_has_extension_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_has_extension_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_glinterface_has_extension_0_Pre(parms);
			}
			var glInterface = parms.glInterface;
			var extension_Length = parms.extension.length*4+1
			var extension = CanvasKit._malloc(extension_Length);
			CanvasKit.stringToUTF8(parms.extension, extension, extension_Length);
			var ret = CanvasKit._gr_glinterface_has_extension(glInterface, extension);
			if((<any>SkiaSharp.ApiOverride).gr_glinterface_has_extension_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_glinterface_has_extension_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_new_gl_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_backendtexture_new_gl_0_Return();
			var parms = gr_backendtexture_new_gl_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_new_gl_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_new_gl_0_Pre(parms);
			}
			var width = parms.width;
			var height = parms.height;
			var mipmapped = parms.mipmapped;
			var glInfo = parms.glInfo.marshalNew(CanvasKit);
			var ret = CanvasKit._gr_backendtexture_new_gl(width, height, mipmapped, glInfo);
			var retStruct = new gr_backendtexture_new_gl_0_Return();
			retStruct.glInfo = SkiaSharp.GRGlTextureInfo.unmarshal(glInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_new_gl_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_new_gl_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_backendtexture_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_delete_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_delete(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_is_valid_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_is_valid_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_is_valid_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_is_valid_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_is_valid(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_is_valid_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_is_valid_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_get_width_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_get_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_get_width_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_get_width(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_get_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_get_height_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_get_height_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_height_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_get_height_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_get_height(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_height_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_get_height_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_has_mipmaps_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_has_mipmaps_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_has_mipmaps_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_has_mipmaps_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_has_mipmaps(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_has_mipmaps_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_has_mipmaps_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_get_backend_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendtexture_get_backend_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_backend_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_get_backend_0_Pre(parms);
			}
			var texture = parms.texture;
			var ret = CanvasKit._gr_backendtexture_get_backend(texture);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_backend_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_get_backend_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendtexture_get_gl_textureinfo_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_backendtexture_get_gl_textureinfo_0_Return();
			var parms = gr_backendtexture_get_gl_textureinfo_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_gl_textureinfo_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendtexture_get_gl_textureinfo_0_Pre(parms);
			}
			var texture = parms.texture;
			var glInfo = retStruct.glInfo.marshalNew(CanvasKit);
			var ret = CanvasKit._gr_backendtexture_get_gl_textureinfo(texture, glInfo);
			var retStruct = new gr_backendtexture_get_gl_textureinfo_0_Return();
			retStruct.glInfo = SkiaSharp.GRGlTextureInfo.unmarshal(glInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).gr_backendtexture_get_gl_textureinfo_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendtexture_get_gl_textureinfo_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_backendrendertarget_new_gl_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_backendrendertarget_new_gl_0_Return();
			var parms = gr_backendrendertarget_new_gl_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_new_gl_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_new_gl_0_Pre(parms);
			}
			var width = parms.width;
			var height = parms.height;
			var samples = parms.samples;
			var stencils = parms.stencils;
			var glInfo = parms.glInfo.marshalNew(CanvasKit);
			var ret = CanvasKit._gr_backendrendertarget_new_gl(width, height, samples, stencils, glInfo);
			var retStruct = new gr_backendrendertarget_new_gl_0_Return();
			retStruct.glInfo = SkiaSharp.GRGlFramebufferInfo.unmarshal(glInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_new_gl_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_new_gl_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static gr_backendrendertarget_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_delete_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_delete(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_is_valid_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_is_valid_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_is_valid_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_is_valid_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_is_valid(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_is_valid_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_is_valid_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_width_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_get_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_width_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_get_width(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_height_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_get_height_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_height_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_height_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_get_height(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_height_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_height_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_samples_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_get_samples_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_samples_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_samples_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_get_samples(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_samples_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_samples_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_stencils_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_get_stencils_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_stencils_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_stencils_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_get_stencils(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_stencils_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_stencils_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_backend_0(pParams : number, pReturn : number) : number
		{
			var parms = gr_backendrendertarget_get_backend_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_backend_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_backend_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var ret = CanvasKit._gr_backendrendertarget_get_backend(rendertarget);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_backend_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_backend_0_Post(ret, parms);
			}
			return ret;
		}
		public static gr_backendrendertarget_get_gl_framebufferinfo_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new gr_backendrendertarget_get_gl_framebufferinfo_0_Return();
			var parms = gr_backendrendertarget_get_gl_framebufferinfo_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_gl_framebufferinfo_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_gl_framebufferinfo_0_Pre(parms);
			}
			var rendertarget = parms.rendertarget;
			var glInfo = retStruct.glInfo.marshalNew(CanvasKit);
			var ret = CanvasKit._gr_backendrendertarget_get_gl_framebufferinfo(rendertarget, glInfo);
			var retStruct = new gr_backendrendertarget_get_gl_framebufferinfo_0_Return();
			retStruct.glInfo = SkiaSharp.GRGlFramebufferInfo.unmarshal(glInfo, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_gl_framebufferinfo_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).gr_backendrendertarget_get_gl_framebufferinfo_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_xmlstreamwriter_new_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_xmlstreamwriter_new_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_new_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_new_0_Pre(parms);
			}
			var stream = parms.stream;
			var ret = CanvasKit._sk_xmlstreamwriter_new(stream);
			if((<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_new_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_xmlstreamwriter_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_xmlstreamwriter_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_delete_0_Pre(parms);
			}
			var writer = parms.writer;
			var ret = CanvasKit._sk_xmlstreamwriter_delete(writer);
			if((<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_xmlstreamwriter_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_svgcanvas_create_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_svgcanvas_create_0_Return();
			var parms = sk_svgcanvas_create_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_svgcanvas_create_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_svgcanvas_create_0_Pre(parms);
			}
			var bounds = parms.bounds.marshalNew(CanvasKit);
			var writer = parms.writer;
			var ret = CanvasKit._sk_svgcanvas_create(bounds, writer);
			var retStruct = new sk_svgcanvas_create_0_Return();
			retStruct.bounds = SkiaSharp.SKRect.unmarshal(bounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_svgcanvas_create_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_svgcanvas_create_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_region_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_region_new();
			if((<any>SkiaSharp.ApiOverride).sk_region_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_region_new2_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_new2_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_new2_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_new2_0_Pre(parms);
			}
			var r = parms.r;
			var ret = CanvasKit._sk_region_new2(r);
			if((<any>SkiaSharp.ApiOverride).sk_region_new2_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_new2_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_contains_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_contains_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_contains_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_contains_0_Pre(parms);
			}
			var r = parms.r;
			var region = parms.region;
			var ret = CanvasKit._sk_region_contains(r, region);
			if((<any>SkiaSharp.ApiOverride).sk_region_contains_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_contains_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_contains2_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_contains2_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_contains2_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_contains2_0_Pre(parms);
			}
			var r = parms.r;
			var x = parms.x;
			var y = parms.y;
			var ret = CanvasKit._sk_region_contains2(r, x, y);
			if((<any>SkiaSharp.ApiOverride).sk_region_contains2_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_contains2_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_intersects_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_intersects_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_intersects_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_intersects_0_Pre(parms);
			}
			var r = parms.r;
			var src = parms.src;
			var ret = CanvasKit._sk_region_intersects(r, src);
			if((<any>SkiaSharp.ApiOverride).sk_region_intersects_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_intersects_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_intersects_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_region_intersects_rect_0_Return();
			var parms = sk_region_intersects_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_intersects_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_intersects_rect_0_Pre(parms);
			}
			var r = parms.r;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_region_intersects_rect(r, rect);
			var retStruct = new sk_region_intersects_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRectI.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_region_intersects_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_intersects_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_region_set_region_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_set_region_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_region_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_set_region_0_Pre(parms);
			}
			var r = parms.r;
			var src = parms.src;
			var ret = CanvasKit._sk_region_set_region(r, src);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_region_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_set_region_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_set_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_region_set_rect_0_Return();
			var parms = sk_region_set_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_set_rect_0_Pre(parms);
			}
			var r = parms.r;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_region_set_rect(r, rect);
			var retStruct = new sk_region_set_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRectI.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_set_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_region_set_path_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_set_path_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_path_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_set_path_0_Pre(parms);
			}
			var r = parms.r;
			var t = parms.t;
			var clip = parms.clip;
			var ret = CanvasKit._sk_region_set_path(r, t, clip);
			if((<any>SkiaSharp.ApiOverride).sk_region_set_path_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_set_path_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_op_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_op_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_op_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_op_0_Pre(parms);
			}
			var r = parms.r;
			var left = parms.left;
			var top = parms.top;
			var right = parms.right;
			var bottom = parms.bottom;
			var op = parms.op;
			var ret = CanvasKit._sk_region_op(r, left, top, right, bottom, op);
			if((<any>SkiaSharp.ApiOverride).sk_region_op_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_op_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_op2_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_region_op2_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_op2_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_op2_0_Pre(parms);
			}
			var r = parms.r;
			var src = parms.src;
			var op = parms.op;
			var ret = CanvasKit._sk_region_op2(r, src, op);
			if((<any>SkiaSharp.ApiOverride).sk_region_op2_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_op2_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_region_get_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_region_get_bounds_0_Return();
			var parms = sk_region_get_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_region_get_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_region_get_bounds_0_Pre(parms);
			}
			var r = parms.r;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_region_get_bounds(r, rect);
			var retStruct = new sk_region_get_bounds_0_Return();
			retStruct.rect = SkiaSharp.SKRectI.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_region_get_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_region_get_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_vertices_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_vertices_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_vertices_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_vertices_unref_0_Pre(parms);
			}
			var cvertices = parms.cvertices;
			var ret = CanvasKit._sk_vertices_unref(cvertices);
			if((<any>SkiaSharp.ApiOverride).sk_vertices_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_vertices_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_vertices_make_copy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_vertices_make_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_vertices_make_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_vertices_make_copy_0_Pre(parms);
			}
			var vmode = parms.vmode;
			var vertexCount = parms.vertexCount;
			var positions = parms.positions;
			var texs = parms.texs;
			var colors = parms.colors;
			var indexCount = parms.indexCount;
			var indices = parms.indices; /* ushort */
			var ret = CanvasKit._sk_vertices_make_copy(vmode, vertexCount, positions, texs, colors, indexCount, indices);
			if((<any>SkiaSharp.ApiOverride).sk_vertices_make_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_vertices_make_copy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_rrect_new();
			if((<any>SkiaSharp.ApiOverride).sk_rrect_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_rrect_new_copy_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_new_copy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_new_copy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_new_copy_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_new_copy(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_new_copy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_new_copy_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_delete_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_delete(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_get_type_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_get_type_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_type_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_get_type_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_get_type(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_type_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_get_type_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_get_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_get_rect_0_Return();
			var parms = sk_rrect_get_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_get_rect_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = retStruct.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_rrect_get_rect(rrect, rect);
			var retStruct = new sk_rrect_get_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_get_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_get_radii_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_get_radii_0_Return();
			var parms = sk_rrect_get_radii_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_radii_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_get_radii_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var corner = parms.corner;
			var radii = retStruct.radii.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_rrect_get_radii(rrect, corner, radii);
			var retStruct = new sk_rrect_get_radii_0_Return();
			retStruct.radii = SkiaSharp.SKPoint.unmarshal(radii, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_radii_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_get_radii_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_get_width_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_get_width_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_width_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_get_width_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_get_width(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_width_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_get_width_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_get_height_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_get_height_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_height_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_get_height_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_get_height(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_get_height_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_get_height_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_set_empty_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_set_empty_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_empty_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_empty_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_set_empty(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_empty_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_empty_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_set_rect_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_set_rect_0_Return();
			var parms = sk_rrect_set_rect_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_rrect_set_rect(rrect, rect);
			var retStruct = new sk_rrect_set_rect_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_set_oval_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_set_oval_0_Return();
			var parms = sk_rrect_set_oval_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_oval_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_oval_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_rrect_set_oval(rrect, rect);
			var retStruct = new sk_rrect_set_oval_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_oval_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_oval_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_set_rect_xy_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_set_rect_xy_0_Return();
			var parms = sk_rrect_set_rect_xy_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_xy_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_xy_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var xRad = parms.xRad;
			var yRad = parms.yRad;
			var ret = CanvasKit._sk_rrect_set_rect_xy(rrect, rect, xRad, yRad);
			var retStruct = new sk_rrect_set_rect_xy_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_xy_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_xy_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_set_nine_patch_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_set_nine_patch_0_Return();
			var parms = sk_rrect_set_nine_patch_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_nine_patch_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_nine_patch_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var leftRad = parms.leftRad;
			var topRad = parms.topRad;
			var rightRad = parms.rightRad;
			var bottomRad = parms.bottomRad;
			var ret = CanvasKit._sk_rrect_set_nine_patch(rrect, rect, leftRad, topRad, rightRad, bottomRad);
			var retStruct = new sk_rrect_set_nine_patch_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_nine_patch_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_nine_patch_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_set_rect_radii_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_set_rect_radii_0_Return();
			var parms = sk_rrect_set_rect_radii_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_radii_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_radii_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var radii = parms.radii;
			var ret = CanvasKit._sk_rrect_set_rect_radii(rrect, rect, radii);
			var retStruct = new sk_rrect_set_rect_radii_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_radii_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_set_rect_radii_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_inset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_inset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_inset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_inset_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_rrect_inset(rrect, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_inset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_inset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_outset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_outset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_outset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_outset_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_rrect_outset(rrect, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_outset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_outset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_offset_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_offset_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_offset_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_offset_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var dx = parms.dx;
			var dy = parms.dy;
			var ret = CanvasKit._sk_rrect_offset(rrect, dx, dy);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_offset_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_offset_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_contains_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_contains_0_Return();
			var parms = sk_rrect_contains_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_contains_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_contains_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var rect = parms.rect.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_rrect_contains(rrect, rect);
			var retStruct = new sk_rrect_contains_0_Return();
			retStruct.rect = SkiaSharp.SKRect.unmarshal(rect, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_contains_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_contains_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_rrect_is_valid_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_rrect_is_valid_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_is_valid_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_is_valid_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var ret = CanvasKit._sk_rrect_is_valid(rrect);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_is_valid_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_is_valid_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_rrect_transform_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_rrect_transform_0_Return();
			var parms = sk_rrect_transform_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_transform_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_rrect_transform_0_Pre(parms);
			}
			var rrect = parms.rrect;
			var matrix = parms.matrix.marshalNew(CanvasKit);
			var dest = parms.dest;
			var ret = CanvasKit._sk_rrect_transform(rrect, matrix, dest);
			var retStruct = new sk_rrect_transform_0_Return();
			retStruct.matrix = SkiaSharp.SKMatrix.unmarshal(matrix, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_rrect_transform_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_rrect_transform_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_ref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_textblob_ref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_ref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_ref_0_Pre(parms);
			}
			var blob = parms.blob;
			var ret = CanvasKit._sk_textblob_ref(blob);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_ref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_ref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_textblob_unref_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_textblob_unref_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_unref_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_unref_0_Pre(parms);
			}
			var blob = parms.blob;
			var ret = CanvasKit._sk_textblob_unref(blob);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_unref_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_unref_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_textblob_get_unique_id_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_textblob_get_unique_id_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_get_unique_id_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_get_unique_id_0_Pre(parms);
			}
			var blob = parms.blob;
			var ret = CanvasKit._sk_textblob_get_unique_id(blob);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_get_unique_id_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_get_unique_id_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_textblob_get_bounds_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_get_bounds_0_Return();
			var parms = sk_textblob_get_bounds_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_get_bounds_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_get_bounds_0_Pre(parms);
			}
			var blob = parms.blob;
			var bounds = retStruct.bounds.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_textblob_get_bounds(blob, bounds);
			var retStruct = new sk_textblob_get_bounds_0_Return();
			retStruct.bounds = SkiaSharp.SKRect.unmarshal(bounds, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_get_bounds_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_get_bounds_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_new_0(pParams : number, pReturn : number) : number
		{
			var ret = CanvasKit._sk_textblob_builder_new();
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_new_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_new_0_Post(ret);
			}
			return ret;
		}
		public static sk_textblob_builder_delete_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_textblob_builder_delete_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_delete_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_delete_0_Pre(parms);
			}
			var builder = parms.builder;
			var ret = CanvasKit._sk_textblob_builder_delete(builder);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_delete_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_delete_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_textblob_builder_make_0(pParams : number, pReturn : number) : number
		{
			var parms = sk_textblob_builder_make_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_make_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_make_0_Pre(parms);
			}
			var builder = parms.builder;
			var ret = CanvasKit._sk_textblob_builder_make(builder);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_make_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_make_0_Post(ret, parms);
			}
			return ret;
		}
		public static sk_textblob_builder_alloc_run_text_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_alloc_run_text_0_Return();
			var parms = sk_textblob_builder_alloc_run_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_0_Pre(parms);
			}
			var builder = parms.builder;
			var font = parms.font;
			var count = parms.count;
			var x = parms.x;
			var y = parms.y;
			var textByteCount = parms.textByteCount;
			var lang = parms.lang;
			var bounds = parms.bounds;
			var runbuffer = retStruct.runbuffer.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_textblob_builder_alloc_run_text(builder, font, count, x, y, textByteCount, lang, bounds, runbuffer);
			var retStruct = new sk_textblob_builder_alloc_run_text_0_Return();
			retStruct.runbuffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(runbuffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_alloc_run_text_pos_h_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_alloc_run_text_pos_h_0_Return();
			var parms = sk_textblob_builder_alloc_run_text_pos_h_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_h_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_h_0_Pre(parms);
			}
			var builder = parms.builder;
			var font = parms.font;
			var count = parms.count;
			var y = parms.y;
			var textByteCount = parms.textByteCount;
			var lang = parms.lang;
			var bounds = parms.bounds;
			var runbuffer = retStruct.runbuffer.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_textblob_builder_alloc_run_text_pos_h(builder, font, count, y, textByteCount, lang, bounds, runbuffer);
			var retStruct = new sk_textblob_builder_alloc_run_text_pos_h_0_Return();
			retStruct.runbuffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(runbuffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_h_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_h_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_alloc_run_text_pos_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_alloc_run_text_pos_0_Return();
			var parms = sk_textblob_builder_alloc_run_text_pos_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_0_Pre(parms);
			}
			var builder = parms.builder;
			var font = parms.font;
			var count = parms.count;
			var textByteCount = parms.textByteCount;
			var lang = parms.lang;
			var bounds = parms.bounds;
			var runbuffer = retStruct.runbuffer.marshalNew(CanvasKit);
			var ret = CanvasKit._sk_textblob_builder_alloc_run_text_pos(builder, font, count, textByteCount, lang, bounds, runbuffer);
			var retStruct = new sk_textblob_builder_alloc_run_text_pos_0_Return();
			retStruct.runbuffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(runbuffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_alloc_run_text_pos_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_runbuffer_set_glyphs_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_runbuffer_set_glyphs_0_Return();
			var parms = sk_textblob_builder_runbuffer_set_glyphs_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_glyphs_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_glyphs_0_Pre(parms);
			}
			var buffer = parms.buffer.marshalNew(CanvasKit);
			var glyphs = parms.glyphs;
			var count = parms.count;
			var ret = CanvasKit._sk_textblob_builder_runbuffer_set_glyphs(buffer, glyphs, count);
			var retStruct = new sk_textblob_builder_runbuffer_set_glyphs_0_Return();
			retStruct.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(buffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_glyphs_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_glyphs_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_runbuffer_set_pos_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_runbuffer_set_pos_0_Return();
			var parms = sk_textblob_builder_runbuffer_set_pos_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_0_Pre(parms);
			}
			var buffer = parms.buffer.marshalNew(CanvasKit);
			var pos = parms.pos;
			var count = parms.count;
			var ret = CanvasKit._sk_textblob_builder_runbuffer_set_pos(buffer, pos, count);
			var retStruct = new sk_textblob_builder_runbuffer_set_pos_0_Return();
			retStruct.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(buffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_runbuffer_set_pos_points_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_runbuffer_set_pos_points_0_Return();
			var parms = sk_textblob_builder_runbuffer_set_pos_points_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_points_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_points_0_Pre(parms);
			}
			var buffer = parms.buffer.marshalNew(CanvasKit);
			var pos = parms.pos;
			var count = parms.count;
			var ret = CanvasKit._sk_textblob_builder_runbuffer_set_pos_points(buffer, pos, count);
			var retStruct = new sk_textblob_builder_runbuffer_set_pos_points_0_Return();
			retStruct.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(buffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_points_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_pos_points_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_runbuffer_set_utf8_text_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_runbuffer_set_utf8_text_0_Return();
			var parms = sk_textblob_builder_runbuffer_set_utf8_text_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_utf8_text_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_utf8_text_0_Pre(parms);
			}
			var buffer = parms.buffer.marshalNew(CanvasKit);
			var text = parms.text;
			var count = parms.count;
			var ret = CanvasKit._sk_textblob_builder_runbuffer_set_utf8_text(buffer, text, count);
			var retStruct = new sk_textblob_builder_runbuffer_set_utf8_text_0_Return();
			retStruct.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(buffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_utf8_text_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_utf8_text_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
		public static sk_textblob_builder_runbuffer_set_clusters_0(pParams : number, pReturn : number) : number
		{
			var retStruct = new sk_textblob_builder_runbuffer_set_clusters_0_Return();
			var parms = sk_textblob_builder_runbuffer_set_clusters_0_Params.unmarshal(pParams);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_clusters_0_Pre)
			{
				(<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_clusters_0_Pre(parms);
			}
			var buffer = parms.buffer.marshalNew(CanvasKit);
			var clusters = parms.clusters;
			var count = parms.count;
			var ret = CanvasKit._sk_textblob_builder_runbuffer_set_clusters(buffer, clusters, count);
			var retStruct = new sk_textblob_builder_runbuffer_set_clusters_0_Return();
			retStruct.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(buffer, CanvasKit);
			if((<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_clusters_0_Post)
			{
				ret = (<any>SkiaSharp.ApiOverride).sk_textblob_builder_runbuffer_set_clusters_0_Post(ret, parms, retStruct);
			}
			retStruct.marshal(pReturn);
			return ret;
		}
	}
}
