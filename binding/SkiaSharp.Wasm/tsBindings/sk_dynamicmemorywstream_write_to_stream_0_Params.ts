/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_dynamicmemorywstream_write_to_stream_0_Params
	{
		/* Pack=4 */
		cstream : number;
		dst : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_dynamicmemorywstream_write_to_stream_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_dynamicmemorywstream_write_to_stream_0_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
