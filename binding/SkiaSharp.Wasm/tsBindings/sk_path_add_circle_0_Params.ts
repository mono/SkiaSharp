/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_circle_0_Params
	{
		/* Pack=4 */
		t : number;
		x : number;
		y : number;
		radius : number;
		dir : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_circle_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_circle_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.dir = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
