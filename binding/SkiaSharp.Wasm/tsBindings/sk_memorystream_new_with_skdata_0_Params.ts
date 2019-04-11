/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_memorystream_new_with_skdata_0_Params
	{
		/* Pack=4 */
		data : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_memorystream_new_with_skdata_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_memorystream_new_with_skdata_0_Params();
			
			{
				ret.data = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
