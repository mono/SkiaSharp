/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_maskfilter_new_blur_0_Params
	{
		/* Pack=4 */
		style : number;
		sigma : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_maskfilter_new_blur_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_maskfilter_new_blur_0_Params();
			
			{
				ret.style = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.sigma = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
