/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_matrix_convolution_0_Return
	{
		/* Pack=4 */
		kernelSize : SkiaSharp.SKSizeI;
		kernelOffset : SkiaSharp.SKPointI;
		public constructor()
		{
			this.kernelSize = new SkiaSharp.SKSizeI();
			this.kernelOffset = new SkiaSharp.SKPointI();
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
			this.kernelSize.marshal(pData + 0);
			this.kernelOffset.marshal(pData + 8);
		}
	}
}
