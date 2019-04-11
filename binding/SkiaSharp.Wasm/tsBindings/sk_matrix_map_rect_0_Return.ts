/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_map_rect_0_Return
	{
		/* Pack=4 */
		matrix : SkiaSharp.SKMatrix;
		dest : SkiaSharp.SKRect;
		source : SkiaSharp.SKRect;
		public constructor()
		{
			this.matrix = new SkiaSharp.SKMatrix();
			this.dest = new SkiaSharp.SKRect();
			this.source = new SkiaSharp.SKRect();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(68);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.matrix.marshal(pData + 0);
			this.dest.marshal(pData + 36);
			this.source.marshal(pData + 52);
		}
	}
}
