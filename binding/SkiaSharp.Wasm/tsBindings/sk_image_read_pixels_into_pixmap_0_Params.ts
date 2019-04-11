/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_read_pixels_into_pixmap_0_Params
	{
		/* Pack=4 */
		image : number;
		dst : number;
		srcX : number;
		srcY : number;
		cachingHint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_read_pixels_into_pixmap_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_read_pixels_into_pixmap_0_Params();
			
			{
				ret.image = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.srcX = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.srcY = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.cachingHint = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
