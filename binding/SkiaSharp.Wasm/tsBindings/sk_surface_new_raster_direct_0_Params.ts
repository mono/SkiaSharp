/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_new_raster_direct_0_Params
	{
		/* Pack=4 */
		info : SkiaSharp.SKImageInfoNative;
		pixels : number;
		rowBytes : number;
		releaseProc : number;
		context : number;
		props : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_new_raster_direct_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_new_raster_direct_0_Params();
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 0);
			}
			
			{
				ret.pixels = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.releaseProc = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.context = Number(memoryContext.getValue(pData + 32, "*"));
			}
			
			{
				ret.props = Number(memoryContext.getValue(pData + 36, "*"));
			}
			return ret;
		}
	}
}
