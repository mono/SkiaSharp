/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_is_numerical_transfer_fn_0_Return
	{
		/* Pack=4 */
		fn : SkiaSharp.SKColorSpaceTransferFn;
		public constructor()
		{
			this.fn = new SkiaSharp.SKColorSpaceTransferFn();
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
			this.fn.marshal(pData + 0);
		}
	}
}
