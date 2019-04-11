/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_convert_conic_to_quads_0_Return
	{
		/* Pack=4 */
		p0 : SkiaSharp.SKPoint;
		p1 : SkiaSharp.SKPoint;
		p2 : SkiaSharp.SKPoint;
		public constructor()
		{
			this.p0 = new SkiaSharp.SKPoint();
			this.p1 = new SkiaSharp.SKPoint();
			this.p2 = new SkiaSharp.SKPoint();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(24);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.p0.marshal(pData + 0);
			this.p1.marshal(pData + 8);
			this.p2.marshal(pData + 16);
		}
	}
}
