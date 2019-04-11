/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_valid_subset_0_Return
	{
		/* Pack=4 */
		desiredSubset : SkiaSharp.SKRectI;
		public constructor()
		{
			this.desiredSubset = new SkiaSharp.SKRectI();
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
			this.desiredSubset.marshal(pData + 0);
		}
	}
}
