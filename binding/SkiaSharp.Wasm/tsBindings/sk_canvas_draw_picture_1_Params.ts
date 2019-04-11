/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_picture_1_Params
	{
		/* Pack=4 */
		t : number;
		pict : number;
		matZero : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_picture_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_picture_1_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.pict = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.matZero = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
