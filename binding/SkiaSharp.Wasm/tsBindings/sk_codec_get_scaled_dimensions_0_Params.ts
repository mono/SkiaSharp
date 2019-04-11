/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_scaled_dimensions_0_Params
	{
		/* Pack=4 */
		codec : number;
		desiredScale : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_scaled_dimensions_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_scaled_dimensions_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.desiredScale = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
