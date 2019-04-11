/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_make_with_filter_0_Return
	{
		/* Pack=4 */
		subset : SkiaSharp.SKRectI;
		clipbounds : SkiaSharp.SKRectI;
		outSubset : SkiaSharp.SKRectI;
		outOffset : SkiaSharp.SKPoint;
		public constructor()
		{
			this.subset = new SkiaSharp.SKRectI();
			this.clipbounds = new SkiaSharp.SKRectI();
			this.outSubset = new SkiaSharp.SKRectI();
			this.outOffset = new SkiaSharp.SKPoint();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(56);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.subset.marshal(pData + 0);
			this.clipbounds.marshal(pData + 16);
			this.outSubset.marshal(pData + 32);
			this.outOffset.marshal(pData + 48);
		}
	}
}
