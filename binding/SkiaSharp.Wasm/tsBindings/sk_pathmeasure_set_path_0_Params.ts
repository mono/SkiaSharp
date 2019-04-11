/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_set_path_0_Params
	{
		/* Pack=4 */
		pathMeasure : number;
		path : number;
		forceClosed : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_set_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_set_path_0_Params();
			
			{
				ret.pathMeasure = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.path = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.forceClosed = Boolean(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
