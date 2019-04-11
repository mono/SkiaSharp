/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_get_pos_tan_1_Params
	{
		/* Pack=4 */
		pathMeasure : number;
		distance : number;
		positionZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_get_pos_tan_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_get_pos_tan_1_Params();
			
			{
				ret.pathMeasure = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.distance = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.positionZero = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
