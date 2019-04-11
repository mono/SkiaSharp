/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_read_pixels_0_Params
	{
		/* Pack=4 */
		surface : number;
		dstInfo : SkiaSharp.SKImageInfoNative;
		dstPixels : number;
		dstRowBytes : number;
		srcX : number;
		srcY : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_read_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_read_pixels_0_Params();
			
			{
				ret.surface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dstInfo = SkiaSharp.SKImageInfoNative.unmarshal(pData + 4);
			}
			
			{
				ret.dstPixels = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.dstRowBytes = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.srcX = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			
			{
				ret.srcY = Number(memoryContext.getValue(pData + 36, "i32"));
			}
			return ret;
		}
	}
}
