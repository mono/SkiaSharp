/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_region_set_rect_0_Params
	{
		/* Pack=4 */
		r : number;
		rect : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_region_set_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_region_set_rect_0_Params();
			
			{
				ret.r = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRectI.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
