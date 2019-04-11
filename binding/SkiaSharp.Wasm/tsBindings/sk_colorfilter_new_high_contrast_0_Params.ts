/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_high_contrast_0_Params
	{
		/* Pack=4 */
		config : SkiaSharp.SKHighContrastConfig;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_high_contrast_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_high_contrast_0_Params();
			
			{
				ret.config = SkiaSharp.SKHighContrastConfig.unmarshal(pData + 0);
			}
			return ret;
		}
	}
}
