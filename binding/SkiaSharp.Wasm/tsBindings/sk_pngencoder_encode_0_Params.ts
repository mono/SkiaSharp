/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pngencoder_encode_0_Params
	{
		/* Pack=4 */
		dst : number;
		src : number;
		options : SkiaSharp.SKPngEncoderOptions;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pngencoder_encode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pngencoder_encode_0_Params();
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.options = SkiaSharp.SKPngEncoderOptions.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
