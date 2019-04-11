/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_start_scanline_decode_0_Params
	{
		/* Pack=4 */
		codec : number;
		info : SkiaSharp.SKImageInfoNative;
		options : SkiaSharp.SKCodecOptionsInternal;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_start_scanline_decode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_start_scanline_decode_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 4);
			}
			
			{
				ret.options = SkiaSharp.SKCodecOptionsInternal.unmarshal(pData + 24);
			}
			return ret;
		}
	}
}
