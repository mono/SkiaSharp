/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_3dview_translate_0_Params
	{
		/* Pack=4 */
		cview : number;
		x : number;
		y : number;
		z : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_3dview_translate_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_3dview_translate_0_Params();
			
			{
				ret.cview = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.z = Number(memoryContext.getValue(pData + 12, "float"));
			}
			return ret;
		}
	}
}
