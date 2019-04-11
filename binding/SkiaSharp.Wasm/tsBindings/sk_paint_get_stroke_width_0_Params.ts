/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_stroke_width_0_Params
	{
		/* Pack=4 */
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_stroke_width_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_stroke_width_0_Params();
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
