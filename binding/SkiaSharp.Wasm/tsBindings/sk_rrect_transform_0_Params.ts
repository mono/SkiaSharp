/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_rrect_transform_0_Params
	{
		/* Pack=4 */
		rrect : number;
		matrix : SkiaSharp.SKMatrix;
		dest : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_rrect_transform_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_rrect_transform_0_Params();
			
			{
				ret.rrect = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 4);
			}
			
			{
				ret.dest = Number(memoryContext.getValue(pData + 40, "*"));
			}
			return ret;
		}
	}
}
