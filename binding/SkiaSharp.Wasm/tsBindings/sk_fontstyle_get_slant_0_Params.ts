/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontstyle_get_slant_0_Params
	{
		/* Pack=4 */
		fs : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontstyle_get_slant_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontstyle_get_slant_0_Params();
			
			{
				ret.fs = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
