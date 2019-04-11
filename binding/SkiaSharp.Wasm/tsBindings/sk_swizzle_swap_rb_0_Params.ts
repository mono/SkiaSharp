/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_swizzle_swap_rb_0_Params
	{
		/* Pack=4 */
		dest : number;
		src : number;
		count : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_swizzle_swap_rb_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_swizzle_swap_rb_0_Params();
			
			{
				ret.dest = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
