/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontstyleset_get_style_0_Params
	{
		/* Pack=4 */
		fss : number;
		index : number;
		fs : number;
		style : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontstyleset_get_style_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontstyleset_get_style_0_Params();
			
			{
				ret.fss = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.index = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fs = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.style = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
