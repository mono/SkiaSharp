/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_drawable_0_Return
	{
		/* Pack=4 */
		mat : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.mat = new SkiaSharp.SKMatrix();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(36);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.mat.marshal(pData + 0);
		}
	}
}
