/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_measure_text_0_Params
	{
		/* Pack=4 */
		t : number;
		text : number;
		length : number;
		bounds : SkiaSharp.SKRect;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_measure_text_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_measure_text_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.text = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.bounds = SkiaSharp.SKRect.unmarshal(pData + 12);
			}
			return ret;
		}
	}
}
