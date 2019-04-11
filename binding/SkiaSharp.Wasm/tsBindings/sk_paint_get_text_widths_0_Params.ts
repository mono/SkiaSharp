/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_text_widths_0_Params
	{
		/* Pack=4 */
		cpaint : number;
		text : number;
		byteLength : number;
		widths : number;
		bounds : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_text_widths_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_text_widths_0_Params();
			
			{
				ret.cpaint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.text = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.byteLength = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.widths = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.bounds = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
