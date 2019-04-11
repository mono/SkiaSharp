/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendrendertarget_new_gl_0_Params
	{
		/* Pack=4 */
		width : number;
		height : number;
		samples : number;
		stencils : number;
		glInfo : SkiaSharp.GRGlFramebufferInfo;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_backendrendertarget_new_gl_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_backendrendertarget_new_gl_0_Params();
			
			{
				ret.width = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.height = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.samples = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.stencils = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.glInfo = SkiaSharp.GRGlFramebufferInfo.unmarshal(pData + 16);
			}
			return ret;
		}
	}
}
