/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_matrix_0_Params
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		quality : number;
		input : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_matrix_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_matrix_0_Params();
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.quality = Number(memoryContext.getValue(pData + 36, "i32"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 40, "*"));
			}
			return ret;
		}
	}
}
