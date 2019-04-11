/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_text_0_Params
	{
		/* Pack=4 */
		t : number;
		text_Length : number;
		text : Array<number>;
		len : number;
		x : number;
		y : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_text_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_text_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.text_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*"); /*byte 1 False*/
				if(pArray !== 0)
				{
					ret.text = new Array<number>();
					for(var i=0; i<ret.text_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 1, "i8");
						ret.text.push(Number(value));
					}
				}
				else
				
				{
					ret.text = null;
				}
			}
			
			{
				ret.len = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
