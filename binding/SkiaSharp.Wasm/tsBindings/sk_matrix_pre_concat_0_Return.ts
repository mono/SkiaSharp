/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_pre_concat_0_Return
	{
		/* Pack=4 */
		target : SkiaSharp.SKMatrix;
		matrix : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.target = new SkiaSharp.SKMatrix();
			this.matrix = new SkiaSharp.SKMatrix();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(72);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.target.marshal(pData + 0);
			this.matrix.marshal(pData + 36);
		}
	}
}
