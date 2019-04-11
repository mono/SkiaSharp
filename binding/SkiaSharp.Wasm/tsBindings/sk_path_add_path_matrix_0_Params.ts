/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_path_matrix_0_Params
	{
		/* Pack=4 */
		t : number;
		other : number;
		matrix : SkiaSharp.SKMatrix;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_path_matrix_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_path_matrix_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.other = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 8);
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 44, "i32"));
			}
			return ret;
		}
	}
}
