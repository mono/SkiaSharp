/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontmgr_match_family_0_Params
	{
		/* Pack=4 */
		fontmgr : number;
		familyName : string;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontmgr_match_family_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontmgr_match_family_0_Params();
			
			{
				ret.fontmgr = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				var ptr = memoryContext.getValue(pData + 4, "*");
				if(ptr !== 0)
				{
					ret.familyName = String(memoryContext.UTF8ToString(ptr));
				}
				else
				
				{
					ret.familyName = null;
				}
			}
			return ret;
		}
	}
}
