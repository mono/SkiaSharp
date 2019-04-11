/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_arc_to_with_oval_0_Params
	{
		/* Pack=4 */
		t : number;
		oval : SkiaSharp.SKRect;
		startAngle : number;
		sweepAngle : number;
		forceMoveTo : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_arc_to_with_oval_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_arc_to_with_oval_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.oval = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.startAngle = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.sweepAngle = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.forceMoveTo = Boolean(memoryContext.getValue(pData + 28, "i32"));
			}
			return ret;
		}
	}
}
