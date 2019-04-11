/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_bitmap_0_Params
	{
		/* Pack=4 */
		src : number;
		tmx : number;
		tmy : number;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_bitmap_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_bitmap_0_Params();
			
			{
				ret.src = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.tmx = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.tmy = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 12);
			}
			return ret;
		}
	}
}
