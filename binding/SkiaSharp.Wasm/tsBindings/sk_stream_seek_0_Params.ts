/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_stream_seek_0_Params
	{
		/* Pack=4 */
		stream : number;
		position : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_stream_seek_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_stream_seek_0_Params();
			
			{
				ret.stream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.position = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
