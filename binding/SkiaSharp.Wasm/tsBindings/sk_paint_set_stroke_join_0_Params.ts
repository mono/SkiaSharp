/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_set_stroke_join_0_Params
	{
		/* Pack=4 */
		t : number;
		join : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_set_stroke_join_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_set_stroke_join_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.join = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
