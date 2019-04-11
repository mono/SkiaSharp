/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_transform_0_Params
	{
		/* Pack=4 */
		t : number;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_transform_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_transform_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
