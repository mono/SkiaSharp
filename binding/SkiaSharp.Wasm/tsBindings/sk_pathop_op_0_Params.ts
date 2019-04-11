/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathop_op_0_Params
	{
		/* Pack=4 */
		one : number;
		two : number;
		op : number;
		result : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathop_op_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathop_op_0_Params();
			
			{
				ret.one = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.two = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.op = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.result = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
