/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_scanlines_0_Params
	{
		/* Pack=4 */
		codec : number;
		dst : number;
		countLines : number;
		rowBytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_scanlines_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_scanlines_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.countLines = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
