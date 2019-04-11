/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_image_rect_0_Return
	{
		/* Pack=4 */
		src : SkiaSharp.SKRect;
		dest : SkiaSharp.SKRect;
		public constructor()
		{
			this.src = new SkiaSharp.SKRect();
			this.dest = new SkiaSharp.SKRect();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(32);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.src.marshal(pData + 0);
			this.dest.marshal(pData + 16);
		}
	}
}
