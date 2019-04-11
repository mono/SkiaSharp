/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_region_op_0_Params
	{
		/* Pack=4 */
		r : number;
		left : number;
		top : number;
		right : number;
		bottom : number;
		op : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_region_op_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_region_op_0_Params();
			
			{
				ret.r = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.left = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.top = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.right = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.bottom = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.op = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			return ret;
		}
	}
}
