/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendtexture_get_gl_textureinfo_0_Return
	{
		/* Pack=4 */
		glInfo : SkiaSharp.GRGlTextureInfo;
		public constructor()
		{
			this.glInfo = new SkiaSharp.GRGlTextureInfo();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(12);
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
