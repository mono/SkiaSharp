/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_set_stroke_width_0_Params
	{
		/* Pack=4 */
		t : number;
		width : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_set_stroke_width_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_set_stroke_width_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.width = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
