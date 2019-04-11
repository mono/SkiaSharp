/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_memorystream_set_memory_0_Params
	{
		/* Pack=4 */
		s : number;
		data : number;
		length : number;
		copyData : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_memorystream_set_memory_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_memorystream_set_memory_0_Params();
			
			{
				ret.s = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.data = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.copyData = Boolean(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
