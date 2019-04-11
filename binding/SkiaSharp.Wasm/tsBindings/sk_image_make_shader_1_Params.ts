/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_make_shader_1_Params
	{
		/* Pack=4 */
		image : number;
		tileX : number;
		tileY : number;
		localMatrixZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_make_shader_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_make_shader_1_Params();
			
			{
				ret.image = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.tileX = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.tileY = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.localMatrixZero = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
