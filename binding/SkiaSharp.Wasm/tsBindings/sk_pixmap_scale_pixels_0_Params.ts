/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_scale_pixels_0_Params
	{
		/* Pack=4 */
		cpixmap : number;
		dst : number;
		quality : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_scale_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_scale_pixels_0_Params();
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.quality = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
