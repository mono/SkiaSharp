/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_set_filter_quality_0_Params
	{
		/* Pack=4 */
		t : number;
		filterQuality : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_set_filter_quality_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_set_filter_quality_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.filterQuality = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
