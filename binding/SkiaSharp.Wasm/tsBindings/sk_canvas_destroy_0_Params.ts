/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_destroy_0_Params
	{
		/* Pack=4 */
		canvas : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_destroy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_destroy_0_Params();
			
			{
				ret.canvas = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
