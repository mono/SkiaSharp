/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_get_device_clip_bounds_0_Return
	{
		/* Pack=4 */
		cbounds : SkiaSharp.SKRectI;
		public constructor()
		{
			this.cbounds = new SkiaSharp.SKRectI();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(16);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.cbounds.marshal(pData + 0);
		}
	}
}
