/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_rrect_contains_0_Return
	{
		/* Pack=4 */
		rect : SkiaSharp.SKRect;
		public constructor()
		{
			this.rect = new SkiaSharp.SKRect();
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
			this.rect.marshal(pData + 0);
		}
	}
}
