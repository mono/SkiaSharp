/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_nway_canvas_add_canvas_0_Params
	{
		/* Pack=4 */
		t : number;
		canvas : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_nway_canvas_add_canvas_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_nway_canvas_add_canvas_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.canvas = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
