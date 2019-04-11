/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Return
	{
		/* Pack=4 */
		coeffs : SkiaSharp.SKColorSpaceTransferFn;
		public constructor()
		{
			this.coeffs = new SkiaSharp.SKColorSpaceTransferFn();
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
			this.coeffs.marshal(pData + 0);
		}
	}
}
