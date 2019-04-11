/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_fontmetrics_0_Params
	{
		/* Pack=4 */
		t : number;
		scale : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_fontmetrics_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_fontmetrics_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.scale = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
