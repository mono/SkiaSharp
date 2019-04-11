/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_maskfilter_new_blur_with_flags_0_Params
	{
		/* Pack=4 */
		style : number;
		sigma : number;
		occluder : SkiaSharp.SKRect;
		respectCTM : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_maskfilter_new_blur_with_flags_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_maskfilter_new_blur_with_flags_0_Params();
			
			{
				ret.style = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.sigma = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.occluder = SkiaSharp.SKRect.unmarshal(pData + 8);
			}
			
			{
				ret.respectCTM = Boolean(memoryContext.getValue(pData + 24, "i32"));
			}
			return ret;
		}
	}
}
