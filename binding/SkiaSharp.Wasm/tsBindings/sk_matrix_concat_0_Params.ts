/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_concat_0_Params
	{
		/* Pack=4 */
		target : SkiaSharp.SKMatrix;
		first : SkiaSharp.SKMatrix;
		second : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix_concat_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix_concat_0_Params();
			
			{
				ret.target = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.first = SkiaSharp.SKMatrix.unmarshal(pData + 36);
			}
			
			{
				ret.second = SkiaSharp.SKMatrix.unmarshal(pData + 72);
			}
			return ret;
		}
	}
}
