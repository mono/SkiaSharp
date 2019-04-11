/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_rrect_set_nine_patch_0_Params
	{
		/* Pack=4 */
		rrect : number;
		rect : SkiaSharp.SKRect;
		leftRad : number;
		topRad : number;
		rightRad : number;
		bottomRad : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_rrect_set_nine_patch_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_rrect_set_nine_patch_0_Params();
			
			{
				ret.rrect = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.leftRad = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.topRad = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.rightRad = Number(memoryContext.getValue(pData + 28, "float"));
			}
			
			{
				ret.bottomRad = Number(memoryContext.getValue(pData + 32, "float"));
			}
			return ret;
		}
	}
}
