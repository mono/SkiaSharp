/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_map_vector_0_Params
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		x : number;
		y : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix_map_vector_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix_map_vector_0_Params();
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 36, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 40, "float"));
			}
			return ret;
		}
	}
}
