/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_set_convexity_0_Params
	{
		/* Pack=4 */
		cpath : number;
		convexity : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_set_convexity_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_set_convexity_0_Params();
			
			{
				ret.cpath = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.convexity = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
