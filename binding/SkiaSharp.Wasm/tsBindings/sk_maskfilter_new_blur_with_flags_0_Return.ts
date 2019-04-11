/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_maskfilter_new_blur_with_flags_0_Return
	{
		/* Pack=4 */
		occluder : SkiaSharp.SKRect;
		public constructor()
		{
			this.occluder = new SkiaSharp.SKRect();
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
			this.occluder.marshal(pData + 0);
		}
	}
}
