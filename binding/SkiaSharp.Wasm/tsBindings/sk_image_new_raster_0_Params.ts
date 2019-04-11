/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_raster_0_Params
	{
		/* Pack=4 */
		pixmap : number;
		releaseProc : number;
		context : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_raster_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_raster_0_Params();
			
			{
				ret.pixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.releaseProc = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.context = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
