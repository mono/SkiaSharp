/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_data_new_subset_0_Params
	{
		/* Pack=4 */
		src : number;
		offset : number;
		length : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_data_new_subset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_data_new_subset_0_Params();
			
			{
				ret.src = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.offset = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
