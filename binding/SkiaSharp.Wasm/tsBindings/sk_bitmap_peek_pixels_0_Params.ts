/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_peek_pixels_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		cpixmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_peek_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_peek_pixels_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
