/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_dynamicmemorywstream_copy_to_0_Params
	{
		/* Pack=4 */
		cstream : number;
		data : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_dynamicmemorywstream_copy_to_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_dynamicmemorywstream_copy_to_0_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.data = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
