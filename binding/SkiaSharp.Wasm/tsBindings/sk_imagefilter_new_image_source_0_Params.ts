/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_image_source_0_Params
	{
		/* Pack=4 */
		image : number;
		srcRect : SkiaSharp.SKRect;
		dstRect : SkiaSharp.SKRect;
		filterQuality : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_image_source_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_image_source_0_Params();
			
			{
				ret.image = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.srcRect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.dstRect = SkiaSharp.SKRect.unmarshal(pData + 20);
			}
			
			{
				ret.filterQuality = Number(memoryContext.getValue(pData + 36, "i32"));
			}
			return ret;
		}
	}
}
