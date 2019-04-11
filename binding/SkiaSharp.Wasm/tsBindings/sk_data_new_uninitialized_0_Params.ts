/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_data_new_uninitialized_0_Params
	{
		/* Pack=4 */
		size : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_data_new_uninitialized_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_data_new_uninitialized_0_Params();
			
			{
				ret.size = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
