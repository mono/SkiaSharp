/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_2d_line_0_Params
	{
		/* Pack=4 */
		width : number;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_2d_line_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_2d_line_0_Params();
			
			{
				ret.width = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
