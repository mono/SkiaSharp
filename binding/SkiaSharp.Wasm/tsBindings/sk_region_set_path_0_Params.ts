/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_region_set_path_0_Params
	{
		/* Pack=4 */
		r : number;
		t : number;
		clip : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_region_set_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_region_set_path_0_Params();
			
			{
				ret.r = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.t = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.clip = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
