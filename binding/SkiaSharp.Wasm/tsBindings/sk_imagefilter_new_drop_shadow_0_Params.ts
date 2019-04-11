/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_drop_shadow_0_Params
	{
		/* Pack=4 */
		dx : number;
		dy : number;
		sigmaX : number;
		sigmaY : number;
		color : SkiaSharp.SKColor;
		shadowMode : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_drop_shadow_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_drop_shadow_0_Params();
			
			{
				ret.dx = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.dy = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.sigmaX = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.sigmaY = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 16);
			}
			
			{
				ret.shadowMode = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
