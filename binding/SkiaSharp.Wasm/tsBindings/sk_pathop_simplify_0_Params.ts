/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathop_simplify_0_Params
	{
		/* Pack=4 */
		path : number;
		result : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathop_simplify_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathop_simplify_0_Params();
			
			{
				ret.path = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.result = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
