/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_image_source_0_Return
	{
		/* Pack=4 */
		srcRect : SkiaSharp.SKRect;
		dstRect : SkiaSharp.SKRect;
		public constructor()
		{
			this.srcRect = new SkiaSharp.SKRect();
			this.dstRect = new SkiaSharp.SKRect();
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
			this.srcRect.marshal(pData + 0);
			this.dstRect.marshal(pData + 16);
		}
	}
}
