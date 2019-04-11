/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_3dview_apply_to_canvas_0_Params
	{
		/* Pack=4 */
		cview : number;
		ccanvas : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_3dview_apply_to_canvas_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_3dview_apply_to_canvas_0_Params();
			
			{
				ret.cview = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.ccanvas = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
