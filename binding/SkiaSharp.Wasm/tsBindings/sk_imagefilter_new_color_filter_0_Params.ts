/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_color_filter_0_Params
	{
		/* Pack=4 */
		cf : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_color_filter_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_color_filter_0_Params();
			
			{
				ret.cf = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
