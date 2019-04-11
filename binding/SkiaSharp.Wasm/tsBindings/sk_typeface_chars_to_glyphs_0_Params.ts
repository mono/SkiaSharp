/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_chars_to_glyphs_0_Params
	{
		/* Pack=4 */
		t : number;
		chars : number;
		encoding : number;
		glyphPtr : number;
		glyphCount : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_chars_to_glyphs_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_chars_to_glyphs_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.chars = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.encoding = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.glyphPtr = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.glyphCount = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
