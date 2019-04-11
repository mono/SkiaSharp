/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_stream_move_0_Params
	{
		/* Pack=4 */
		stream : number;
		offset : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_stream_move_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_stream_move_0_Params();
			
			{
				ret.stream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.offset = Number(memoryContext.getValue(pData + 4, "i64"));
			}
			return ret;
		}
	}
}
