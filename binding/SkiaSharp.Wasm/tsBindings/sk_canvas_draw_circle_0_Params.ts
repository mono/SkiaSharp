/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_circle_0_Params
	{
		/* Pack=4 */
		t : number;
		cx : number;
		cy : number;
		radius : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_circle_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_circle_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cx = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.cy = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
