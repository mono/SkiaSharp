/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_pixels_0_Params
	{
		/* Pack=4 */
		codec : number;
		info : SkiaSharp.SKImageInfoNative;
		pixels : number;
		rowBytes : number;
		options : SkiaSharp.SKCodecOptionsInternal;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_pixels_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 4);
			}
			
			{
				ret.pixels = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.options = SkiaSharp.SKCodecOptionsInternal.unmarshal(pData + 32);
			}
			return ret;
		}
	}
}
