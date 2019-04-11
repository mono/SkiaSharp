/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_wstream_write_0_Params
	{
		/* Pack=4 */
		cstream : number;
		buffer : number;
		size : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_wstream_write_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_wstream_write_0_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.buffer = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.size = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
