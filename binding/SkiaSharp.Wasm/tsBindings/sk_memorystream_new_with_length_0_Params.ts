/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_memorystream_new_with_length_0_Params
	{
		/* Pack=4 */
		length : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_memorystream_new_with_length_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_memorystream_new_with_length_0_Params();
			
			{
				ret.length = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
