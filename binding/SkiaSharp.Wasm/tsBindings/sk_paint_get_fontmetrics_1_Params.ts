/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_fontmetrics_1_Params
	{
		/* Pack=4 */
		t : number;
		fontMetricsZero : number;
		scale : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_fontmetrics_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_fontmetrics_1_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.fontMetricsZero = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.scale = Number(memoryContext.getValue(pData + 8, "float"));
			}
			return ret;
		}
	}
}
