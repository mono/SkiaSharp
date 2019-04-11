/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_rrect_start_0_Params
	{
		/* Pack=4 */
		t : number;
		rect : number;
		direction : number;
		startIndex : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_rrect_start_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_rrect_start_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.direction = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.startIndex = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
