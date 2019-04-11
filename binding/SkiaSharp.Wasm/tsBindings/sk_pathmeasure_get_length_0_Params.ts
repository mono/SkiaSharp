/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_get_length_0_Params
	{
		/* Pack=4 */
		pathMeasure : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_get_length_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_get_length_0_Params();
			
			{
				ret.pathMeasure = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
