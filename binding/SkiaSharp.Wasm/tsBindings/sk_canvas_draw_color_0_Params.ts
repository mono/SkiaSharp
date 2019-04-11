/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_color_0_Params
	{
		/* Pack=4 */
		t : number;
		color : SkiaSharp.SKColor;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_color_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_color_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 4);
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
