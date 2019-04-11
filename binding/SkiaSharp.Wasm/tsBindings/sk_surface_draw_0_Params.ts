/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_draw_0_Params
	{
		/* Pack=4 */
		surface : number;
		canvas : number;
		x : number;
		y : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_draw_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_draw_0_Params();
			
			{
				ret.surface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.canvas = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
