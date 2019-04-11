/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_get_segment_0_Params
	{
		/* Pack=4 */
		pathMeasure : number;
		start : number;
		stop : number;
		dst : number;
		startWithMoveTo : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_get_segment_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_get_segment_0_Params();
			
			{
				ret.pathMeasure = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.start = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.stop = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.startWithMoveTo = Boolean(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
