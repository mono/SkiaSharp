/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_magnifier_0_Params
	{
		/* Pack=4 */
		src : SkiaSharp.SKRect;
		inset : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_magnifier_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_magnifier_0_Params();
			
			{
				ret.src = SkiaSharp.SKRect.unmarshal(pData + 0);
			}
			
			{
				ret.inset = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
