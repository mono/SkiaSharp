/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_rawiter_conic_weight_0_Params
	{
		/* Pack=4 */
		iterator : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_rawiter_conic_weight_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_rawiter_conic_weight_0_Params();
			
			{
				ret.iterator = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
