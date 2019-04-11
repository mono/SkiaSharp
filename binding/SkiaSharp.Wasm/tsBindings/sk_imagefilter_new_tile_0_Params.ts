/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_tile_0_Params
	{
		/* Pack=4 */
		src : SkiaSharp.SKRect;
		dst : SkiaSharp.SKRect;
		input : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_tile_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_tile_0_Params();
			
			{
				ret.src = SkiaSharp.SKRect.unmarshal(pData + 0);
			}
			
			{
				ret.dst = SkiaSharp.SKRect.unmarshal(pData + 16);
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 32, "*"));
			}
			return ret;
		}
	}
}
