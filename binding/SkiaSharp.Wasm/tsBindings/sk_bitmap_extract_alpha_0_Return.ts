/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_extract_alpha_0_Return
	{
		/* Pack=4 */
		offset : SkiaSharp.SKPointI;
		public constructor()
		{
			this.offset = new SkiaSharp.SKPointI();
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
			this.offset.marshal(pData + 0);
		}
	}
}
