/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_line_0_Params
	{
		/* Pack=4 */
		t : number;
		x0 : number;
		y0 : number;
		x1 : number;
		y1 : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_line_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_line_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x0 = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.y0 = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.x1 = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.y1 = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 20, "*"));
			}
			return ret;
		}
	}
}
