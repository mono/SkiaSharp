/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_try_alloc_pixels_with_flags_0_Return
	{
		/* Pack=4 */
		requestedInfo : SkiaSharp.SKImageInfoNative;
		public constructor()
		{
			this.requestedInfo = new SkiaSharp.SKImageInfoNative();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(20);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.requestedInfo.marshal(pData + 0);
		}
	}
}
