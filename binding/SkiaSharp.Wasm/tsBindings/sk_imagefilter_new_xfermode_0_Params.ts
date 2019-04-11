/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_xfermode_0_Params
	{
		/* Pack=4 */
		mode : number;
		background : number;
		foreground : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_xfermode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_xfermode_0_Params();
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.background = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.foreground = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
