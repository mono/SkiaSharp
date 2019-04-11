/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_notify_pixels_changed_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_notify_pixels_changed_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_notify_pixels_changed_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
