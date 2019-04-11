/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_text_on_path_1_Params
	{
		/* Pack=4 */
		t : number;
		text : number;
		len : number;
		path : number;
		hOffset : number;
		vOffset : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_text_on_path_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_text_on_path_1_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.text = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.len = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.path = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.hOffset = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.vOffset = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
