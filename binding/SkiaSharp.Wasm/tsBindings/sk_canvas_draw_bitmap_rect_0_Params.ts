/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_bitmap_rect_0_Params
	{
		/* Pack=4 */
		t : number;
		bitmap : number;
		src : SkiaSharp.SKRect;
		dest : SkiaSharp.SKRect;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_bitmap_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_bitmap_rect_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.bitmap = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.src = SkiaSharp.SKRect.unmarshal(pData + 8);
			}
			
			{
				ret.dest = SkiaSharp.SKRect.unmarshal(pData + 24);
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 40, "*"));
			}
			return ret;
		}
	}
}
