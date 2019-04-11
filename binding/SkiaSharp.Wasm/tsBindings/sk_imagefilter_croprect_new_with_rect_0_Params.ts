/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_croprect_new_with_rect_0_Params
	{
		/* Pack=4 */
		rect : SkiaSharp.SKRect;
		flags : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_croprect_new_with_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_croprect_new_with_rect_0_Params();
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 0);
			}
			
			{
				ret.flags = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
