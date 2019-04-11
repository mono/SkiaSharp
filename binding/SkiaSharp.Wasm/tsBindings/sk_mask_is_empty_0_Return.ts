/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_mask_is_empty_0_Return
	{
		/* Pack=4 */
		cmask : SkiaSharp.SKMask;
		public constructor()
		{
			this.cmask = new SkiaSharp.SKMask();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(28);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.cmask.marshal(pData + 0);
		}
	}
}
