/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_3dview_get_matrix_0_Params
	{
		/* Pack=4 */
		cview : number;
		cmatrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_3dview_get_matrix_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_3dview_get_matrix_0_Params();
			
			{
				ret.cview = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cmatrix = SkiaSharp.SKMatrix.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
