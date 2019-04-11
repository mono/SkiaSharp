/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_sum_0_Params
	{
		/* Pack=4 */
		first : number;
		second : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_sum_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_sum_0_Params();
			
			{
				ret.first = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.second = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
