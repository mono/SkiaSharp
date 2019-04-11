/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_is_rrect_0_Params
	{
		/* Pack=4 */
		cpath : number;
		bounds : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_is_rrect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_is_rrect_0_Params();
			
			{
				ret.cpath = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.bounds = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
