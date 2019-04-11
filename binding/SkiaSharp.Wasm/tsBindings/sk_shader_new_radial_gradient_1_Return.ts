/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_radial_gradient_1_Return
	{
		/* Pack=4 */
		center : SkiaSharp.SKPoint;
		public constructor()
		{
			this.center = new SkiaSharp.SKPoint();
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
			this.center.marshal(pData + 0);
		}
	}
}
