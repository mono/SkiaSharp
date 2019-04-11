/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_bitmap_lattice_0_Return
	{
		/* Pack=4 */
		lattice : SkiaSharp.SKLatticeInternal;
		dst : SkiaSharp.SKRect;
		public constructor()
		{
			this.lattice = new SkiaSharp.SKLatticeInternal();
			this.dst = new SkiaSharp.SKRect();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(44);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.lattice.marshal(pData + 0);
			this.dst.marshal(pData + 28);
		}
	}
}
