/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_raster_copy_0_Params
	{
		/* Pack=4 */
		info : SkiaSharp.SKImageInfoNative;
		pixels : number;
		rowBytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_raster_copy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_raster_copy_0_Params();
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 0);
			}
			
			{
				ret.pixels = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
