/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_annotation_0_Params
	{
		/* Pack=4 */
		t : number;
		rect : SkiaSharp.SKRect;
		key_Length : number;
		key : Array<number>;
		value : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_annotation_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_annotation_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.key_Length = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 24, "*");
				if(pArray !== 0)
				{
					ret.key = new Array<number>();
					for(var i=0; i<ret.key_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.key.push(Number(value));
					}
				}
				else
				
				{
					ret.key = null;
				}
			}
			
			{
				ret.value = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
