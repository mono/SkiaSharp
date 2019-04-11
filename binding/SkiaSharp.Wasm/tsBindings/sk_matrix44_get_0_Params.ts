/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_get_0_Params
	{
		/* Pack=4 */
		matrix : number;
		row : number;
		col : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_get_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_get_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.row = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.col = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
