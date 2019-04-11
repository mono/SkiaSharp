/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_alpha_threshold_0_Params
	{
		/* Pack=4 */
		region : number;
		innerThreshold : number;
		outerThreshold : number;
		input : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_alpha_threshold_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_alpha_threshold_0_Params();
			
			{
				ret.region = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.innerThreshold = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.outerThreshold = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
