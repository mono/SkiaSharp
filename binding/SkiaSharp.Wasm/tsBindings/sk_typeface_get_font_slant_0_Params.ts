/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_get_font_slant_0_Params
	{
		/* Pack=4 */
		typeface : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_get_font_slant_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_get_font_slant_0_Params();
			
			{
				ret.typeface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
