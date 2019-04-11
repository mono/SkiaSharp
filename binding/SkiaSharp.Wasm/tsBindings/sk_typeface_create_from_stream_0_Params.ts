/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_create_from_stream_0_Params
	{
		/* Pack=4 */
		stream : number;
		index : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_create_from_stream_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_create_from_stream_0_Params();
			
			{
				ret.stream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.index = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
