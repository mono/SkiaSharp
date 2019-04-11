/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_clip_rrect_with_operation_0_Params
	{
		/* Pack=4 */
		t : number;
		crect : number;
		op : number;
		doAA : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_clip_rrect_with_operation_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_clip_rrect_with_operation_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.crect = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.op = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.doAA = Boolean(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
