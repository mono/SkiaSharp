/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_memorystream_new_with_data_0_Params
	{
		/* Pack=4 */
		data : number;
		length : number;
		copyData : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_memorystream_new_with_data_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_memorystream_new_with_data_0_Params();
			
			{
				ret.data = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.copyData = Boolean(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
