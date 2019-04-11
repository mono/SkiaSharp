/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_bitmap_rect_1_Params
	{
		/* Pack=4 */
		t : number;
		bitmap : number;
		srcZero : number;
		dest : SkiaSharp.SKRect;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_bitmap_rect_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_bitmap_rect_1_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.bitmap = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.srcZero = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.dest = SkiaSharp.SKRect.unmarshal(pData + 12);
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
