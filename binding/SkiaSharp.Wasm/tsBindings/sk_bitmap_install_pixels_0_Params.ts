/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_install_pixels_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		cinfo : SkiaSharp.SKImageInfoNative;
		pixels : number;
		rowBytes : number;
		releaseProc : number;
		context : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_install_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_install_pixels_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(pData + 4);
			}
			
			{
				ret.pixels = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.releaseProc = Number(memoryContext.getValue(pData + 32, "*"));
			}
			
			{
				ret.context = Number(memoryContext.getValue(pData + 36, "*"));
			}
			return ret;
		}
	}
}
