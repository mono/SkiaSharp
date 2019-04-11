/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_make_shader_0_Params
	{
		/* Pack=4 */
		image : number;
		tileX : number;
		tileY : number;
		localMatrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_make_shader_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_make_shader_0_Params();
			
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
				ret.localMatrix = SkiaSharp.SKMatrix.unmarshal(pData + 12);
			}
			return ret;
		}
	}
}
