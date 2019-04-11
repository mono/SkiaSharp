/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_vertices_0_Params
	{
		/* Pack=4 */
		canvas : number;
		vertices : number;
		mode : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_vertices_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_vertices_0_Params();
			
			{
				ret.canvas = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.vertices = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
