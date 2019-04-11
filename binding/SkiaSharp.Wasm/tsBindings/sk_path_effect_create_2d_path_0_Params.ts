/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_2d_path_0_Params
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		path : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_2d_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_2d_path_0_Params();
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 0);
			}
			
			{
				ret.path = Number(memoryContext.getValue(pData + 36, "*"));
			}
			return ret;
		}
	}
}
