/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_drawable_draw_0_Params
	{
		/* Pack=4 */
		d : number;
		c : number;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_drawable_draw_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_drawable_draw_0_Params();
			
			{
				ret.d = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.c = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
