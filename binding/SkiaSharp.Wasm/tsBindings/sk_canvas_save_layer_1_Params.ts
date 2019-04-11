/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_save_layer_1_Params
	{
		/* Pack=4 */
		t : number;
		rectZero : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_save_layer_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_save_layer_1_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rectZero = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
