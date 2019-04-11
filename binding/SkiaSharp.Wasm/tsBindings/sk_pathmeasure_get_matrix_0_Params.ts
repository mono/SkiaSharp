/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_get_matrix_0_Params
	{
		/* Pack=4 */
		pathMeasure : number;
		distance : number;
		flags : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_get_matrix_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_get_matrix_0_Params();
			
			{
				ret.pathMeasure = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.distance = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.flags = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
