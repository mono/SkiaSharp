/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontstyleset_match_style_0_Params
	{
		/* Pack=4 */
		fss : number;
		style : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontstyleset_match_style_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontstyleset_match_style_0_Params();
			
			{
				ret.fss = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.style = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
