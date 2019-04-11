/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_erode_0_Params
	{
		/* Pack=4 */
		radiusX : number;
		radiusY : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_erode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_erode_0_Params();
			
			{
				ret.radiusX = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.radiusY = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
