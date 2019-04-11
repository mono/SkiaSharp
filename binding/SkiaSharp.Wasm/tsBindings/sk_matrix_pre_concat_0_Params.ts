/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_pre_concat_0_Params
	{
		/* Pack=4 */
		target : SkiaSharp.SKMatrix;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix_pre_concat_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix_pre_concat_0_Params();
			
			{
				ret.target = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 36);
			}
			return ret;
		}
	}
}
