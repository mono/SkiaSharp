/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_data_new_with_copy_0_Params
	{
		/* Pack=4 */
		src : number;
		length : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_data_new_with_copy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_data_new_with_copy_0_Params();
			
			{
				ret.src = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
