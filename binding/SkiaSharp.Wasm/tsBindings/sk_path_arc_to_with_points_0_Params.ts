/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_arc_to_with_points_0_Params
	{
		/* Pack=4 */
		t : number;
		x1 : number;
		y1 : number;
		x2 : number;
		y2 : number;
		radius : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_arc_to_with_points_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_arc_to_with_points_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x1 = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.y1 = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.x2 = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.y2 = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 20, "float"));
			}
			return ret;
		}
	}
}
