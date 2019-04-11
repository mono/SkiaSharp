/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_is_srgb_0_Params
	{
		/* Pack=4 */
		cColorSpace : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorspace_is_srgb_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorspace_is_srgb_0_Params();
			
			{
				ret.cColorSpace = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
