/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_rline_to_0_Params
	{
		/* Pack=4 */
		t : number;
		dx : number;
		dy : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_rline_to_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_rline_to_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dx = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.dy = Number(memoryContext.getValue(pData + 8, "float"));
			}
			return ret;
		}
	}
}
