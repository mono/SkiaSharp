/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_arc_0_Params
	{
		/* Pack=4 */
		t : number;
		rect : SkiaSharp.SKRect;
		startAngle : number;
		sweepAngle : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_arc_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_arc_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.startAngle = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.sweepAngle = Number(memoryContext.getValue(pData + 24, "float"));
			}
			return ret;
		}
	}
}
