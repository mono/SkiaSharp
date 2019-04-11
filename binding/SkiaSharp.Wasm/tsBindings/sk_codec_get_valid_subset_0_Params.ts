/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_valid_subset_0_Params
	{
		/* Pack=4 */
		codec : number;
		desiredSubset : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_valid_subset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_valid_subset_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.desiredSubset = SkiaSharp.SKRectI.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
