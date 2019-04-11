/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_jpegencoder_encode_0_Params
	{
		/* Pack=4 */
		dst : number;
		src : number;
		options : SkiaSharp.SKJpegEncoderOptions;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_jpegencoder_encode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_jpegencoder_encode_0_Params();
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.options = SkiaSharp.SKJpegEncoderOptions.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
