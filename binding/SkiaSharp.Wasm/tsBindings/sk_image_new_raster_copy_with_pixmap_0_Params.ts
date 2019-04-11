/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_raster_copy_with_pixmap_0_Params
	{
		/* Pack=4 */
		pixmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_raster_copy_with_pixmap_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_raster_copy_with_pixmap_0_Params();
			
			{
				ret.pixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
