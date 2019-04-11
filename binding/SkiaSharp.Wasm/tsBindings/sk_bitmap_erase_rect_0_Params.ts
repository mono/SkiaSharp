/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_erase_rect_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		color : SkiaSharp.SKColor;
		rect : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_erase_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_erase_rect_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 4);
			}
			
			{
				ret.rect = SkiaSharp.SKRectI.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
