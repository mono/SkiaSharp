/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_path_offset_0_Params
	{
		/* Pack=4 */
		t : number;
		other : number;
		dx : number;
		dy : number;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_path_offset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_path_offset_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.other = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.dx = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.dy = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
