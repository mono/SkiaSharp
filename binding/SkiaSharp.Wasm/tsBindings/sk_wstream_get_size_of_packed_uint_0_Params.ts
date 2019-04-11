/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_wstream_get_size_of_packed_uint_0_Params
	{
		/* Pack=4 */
		value : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_wstream_get_size_of_packed_uint_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_wstream_get_size_of_packed_uint_0_Params();
			
			{
				ret.value = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
