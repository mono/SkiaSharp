/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendrendertarget_get_gl_framebufferinfo_0_Return
	{
		/* Pack=4 */
		glInfo : SkiaSharp.GRGlFramebufferInfo;
		public constructor()
		{
			this.glInfo = new SkiaSharp.GRGlFramebufferInfo();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(8);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.glInfo.marshal(pData + 0);
		}
	}
}
