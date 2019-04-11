/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_radial_gradient_2_Return
	{
		/* Pack=4 */
		center : SkiaSharp.SKPoint;
		matrix : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.center = new SkiaSharp.SKPoint();
			this.matrix = new SkiaSharp.SKMatrix();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(44);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.center.marshal(pData + 0);
			this.matrix.marshal(pData + 8);
		}
	}
}
