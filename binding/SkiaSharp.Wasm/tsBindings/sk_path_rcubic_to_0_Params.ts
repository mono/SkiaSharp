/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_rcubic_to_0_Params
	{
		/* Pack=4 */
		t : number;
		dx0 : number;
		dy0 : number;
		dx1 : number;
		dy1 : number;
		dx2 : number;
		dy2 : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_rcubic_to_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_rcubic_to_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dx0 = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.dy0 = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.dx1 = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.dy1 = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.dx2 = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.dy2 = Number(memoryContext.getValue(pData + 24, "float"));
			}
			return ret;
		}
	}
}
