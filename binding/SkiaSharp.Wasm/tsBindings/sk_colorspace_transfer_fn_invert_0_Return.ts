/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_transfer_fn_invert_0_Return
	{
		/* Pack=4 */
		transfer : SkiaSharp.SKColorSpaceTransferFn;
		inverted : SkiaSharp.SKColorSpaceTransferFn;
		public constructor()
		{
			this.transfer = new SkiaSharp.SKColorSpaceTransferFn();
			this.inverted = new SkiaSharp.SKColorSpaceTransferFn();
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
			this.transfer.marshal(pData + 0);
			this.inverted.marshal(pData + 28);
		}
	}
}
