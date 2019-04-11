/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_new_raster_0_Params
	{
		/* Pack=4 */
		info : SkiaSharp.SKImageInfoNative;
		rowBytes : number;
		props : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_new_raster_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_new_raster_0_Params();
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 0);
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.props = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
