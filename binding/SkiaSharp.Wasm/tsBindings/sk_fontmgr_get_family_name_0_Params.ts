/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontmgr_get_family_name_0_Params
	{
		/* Pack=4 */
		fontmgr : number;
		index : number;
		familyName : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontmgr_get_family_name_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontmgr_get_family_name_0_Params();
			
			{
				ret.fontmgr = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.index = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.familyName = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
