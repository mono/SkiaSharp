/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendtexture_new_gl_0_Params
	{
		/* Pack=4 */
		width : number;
		height : number;
		mipmapped : boolean;
		glInfo : SkiaSharp.GRGlTextureInfo;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_backendtexture_new_gl_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_backendtexture_new_gl_0_Params();
			
			{
				ret.width = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.height = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.mipmapped = Boolean(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.glInfo = SkiaSharp.GRGlTextureInfo.unmarshal(pData + 12);
			}
			return ret;
		}
	}
}
