/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_frame_info_for_index_0_Params
	{
		/* Pack=4 */
		codec : number;
		index : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_frame_info_for_index_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_frame_info_for_index_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.index = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
