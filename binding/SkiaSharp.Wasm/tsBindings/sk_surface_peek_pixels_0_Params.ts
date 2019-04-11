/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_peek_pixels_0_Params
	{
		/* Pack=4 */
		surface : number;
		pixmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_peek_pixels_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_peek_pixels_0_Params();
			
			{
				ret.surface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.pixmap = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
