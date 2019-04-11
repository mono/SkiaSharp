/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_stream_get_memory_base_0_Params
	{
		/* Pack=4 */
		cstream : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_stream_get_memory_base_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_stream_get_memory_base_0_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
