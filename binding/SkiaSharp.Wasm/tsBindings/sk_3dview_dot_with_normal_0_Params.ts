/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_3dview_dot_with_normal_0_Params
	{
		/* Pack=4 */
		cview : number;
		dx : number;
		dy : number;
		dz : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_3dview_dot_with_normal_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_3dview_dot_with_normal_0_Params();
			
			{
				ret.cview = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dx = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.dy = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.dz = Number(memoryContext.getValue(pData + 12, "float"));
			}
			return ret;
		}
	}
}
