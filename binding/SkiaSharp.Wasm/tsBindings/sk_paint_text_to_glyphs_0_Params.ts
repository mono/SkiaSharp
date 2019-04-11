/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_text_to_glyphs_0_Params
	{
		/* Pack=4 */
		cpaint : number;
		text : number;
		byteLength : number;
		glyphs : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_text_to_glyphs_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_text_to_glyphs_0_Params();
			
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
				ret.glyphs = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
