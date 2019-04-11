/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_new_from_data_0_Params
	{
		/* Pack=4 */
		data : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_new_from_data_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_new_from_data_0_Params();
			
			{
				ret.data = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
