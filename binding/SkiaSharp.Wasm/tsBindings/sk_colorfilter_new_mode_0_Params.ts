/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_mode_0_Params
	{
		/* Pack=4 */
		c : SkiaSharp.SKColor;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_mode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_mode_0_Params();
			
			{
				ret.c = SkiaSharp.SKColor.unmarshal(pData + 0);
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
