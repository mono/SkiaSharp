/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_read_pixels_0_Return
	{
		/* Pack=4 */
		dstInfo : SkiaSharp.SKImageInfoNative;
		public constructor()
		{
			this.dstInfo = new SkiaSharp.SKImageInfoNative();
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
			this.dstInfo.marshal(pData + 0);
		}
	}
}
