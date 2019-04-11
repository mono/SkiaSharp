/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix_concat_0_Return
	{
		/* Pack=4 */
		target : SkiaSharp.SKMatrix;
		first : SkiaSharp.SKMatrix;
		second : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.target = new SkiaSharp.SKMatrix();
			this.first = new SkiaSharp.SKMatrix();
			this.second = new SkiaSharp.SKMatrix();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(108);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.target.marshal(pData + 0);
			this.first.marshal(pData + 36);
			this.second.marshal(pData + 72);
		}
	}
}
