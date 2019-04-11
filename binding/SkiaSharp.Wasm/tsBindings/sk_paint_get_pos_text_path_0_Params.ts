/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_pos_text_path_0_Params
	{
		/* Pack=4 */
		t : number;
		text : number;
		length : number;
		points_Length : number;
		points : Array<SkiaSharp.SKPoint>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_pos_text_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_pos_text_path_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.text = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.points_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*");
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
			return ret;
		}
	}
}
