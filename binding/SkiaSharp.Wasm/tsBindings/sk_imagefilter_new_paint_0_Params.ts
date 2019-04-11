/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_paint_0_Params
	{
		/* Pack=4 */
		paint : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_paint_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_paint_0_Params();
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
