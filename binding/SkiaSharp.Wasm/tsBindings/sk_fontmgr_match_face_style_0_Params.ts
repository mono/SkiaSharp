/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontmgr_match_face_style_0_Params
	{
		/* Pack=4 */
		fontmgr : number;
		face : number;
		style : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontmgr_match_face_style_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontmgr_match_face_style_0_Params();
			
			{
				ret.fontmgr = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.face = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.style = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
