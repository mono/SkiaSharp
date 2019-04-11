/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_mask_get_addr_1_0_Params
	{
		/* Pack=4 */
		cmask : SkiaSharp.SKMask;
		x : number;
		y : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_mask_get_addr_1_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_mask_get_addr_1_0_Params();
			
			{
				ret.cmask = SkiaSharp.SKMask.unmarshal(pData + 0);
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			return ret;
		}
	}
}
