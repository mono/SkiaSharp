/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_pos_text_1_Params
	{
		/* Pack=4 */
		t : number;
		text : number;
		len : number;
		points_Length : number;
		points : Array<SkiaSharp.SKPoint>;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_pos_text_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_pos_text_1_Params();
			
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
				ret.points_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*"); /*SkiaSharp.SKPoint 4 False*/
				if(pArray !== 0)
				{
					ret.points = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.points_Length; i++)
					{
						ret.points.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.points = null;
				}
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 20, "*"));
			}
			return ret;
		}
	}
}
