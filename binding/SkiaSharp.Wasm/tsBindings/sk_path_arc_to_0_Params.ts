/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_arc_to_0_Params
	{
		/* Pack=4 */
		t : number;
		rx : number;
		ry : number;
		xAxisRotate : number;
		largeArc : number;
		sweep : number;
		x : number;
		y : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_arc_to_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_arc_to_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rx = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.ry = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.xAxisRotate = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.largeArc = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.sweep = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 28, "float"));
			}
			return ret;
		}
	}
}
