/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_image_nine_0_Return
	{
		/* Pack=4 */
		center : SkiaSharp.SKRectI;
		dst : SkiaSharp.SKRect;
		public constructor()
		{
			this.center = new SkiaSharp.SKRectI();
			this.dst = new SkiaSharp.SKRect();
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
			this.center.marshal(pData + 0);
			this.dst.marshal(pData + 16);
		}
	}
}
