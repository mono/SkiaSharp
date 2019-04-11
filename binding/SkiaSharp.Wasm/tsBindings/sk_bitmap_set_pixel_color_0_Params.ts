/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_set_pixel_color_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		x : number;
		y : number;
		color : SkiaSharp.SKColor;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_set_pixel_color_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_set_pixel_color_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 12);
			}
			return ret;
		}
	}
}
