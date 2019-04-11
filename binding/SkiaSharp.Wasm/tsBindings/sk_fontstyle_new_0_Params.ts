/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontstyle_new_0_Params
	{
		/* Pack=4 */
		weight : number;
		width : number;
		slant : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontstyle_new_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontstyle_new_0_Params();
			
			{
				ret.weight = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.width = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.slant = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
