/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_skew_0_Params
	{
		/* Pack=4 */
		t : number;
		sx : number;
		sy : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_skew_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_skew_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.sx = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.sy = Number(memoryContext.getValue(pData + 8, "float"));
			}
			return ret;
		}
	}
}
