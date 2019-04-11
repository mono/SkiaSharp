/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_map_points_0_Params
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		dst : number;
		src : number;
		count : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix_map_points_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix_map_points_0_Params();
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 36, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 40, "*"));
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 44, "i32"));
			}
			return ret;
		}
	}
}
