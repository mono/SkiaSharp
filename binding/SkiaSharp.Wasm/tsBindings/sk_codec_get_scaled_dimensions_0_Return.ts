/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_scaled_dimensions_0_Return
	{
		/* Pack=4 */
		dimensions : SkiaSharp.SKSizeI;
		public constructor()
		{
			this.dimensions = new SkiaSharp.SKSizeI();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(8);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.dimensions.marshal(pData + 0);
		}
	}
}
