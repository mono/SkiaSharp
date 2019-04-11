/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_install_mask_pixels_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		cmask : SkiaSharp.SKMask;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_install_mask_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_install_mask_pixels_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cmask = SkiaSharp.SKMask.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
