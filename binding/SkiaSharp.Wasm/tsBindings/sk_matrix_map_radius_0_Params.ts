/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_map_radius_0_Params
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		radius : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix_map_radius_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix_map_radius_0_Params();
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 36, "float"));
			}
			return ret;
		}
	}
}
