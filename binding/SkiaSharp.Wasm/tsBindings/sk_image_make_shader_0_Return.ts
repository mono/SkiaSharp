/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_make_shader_0_Return
	{
		/* Pack=4 */
		localMatrix : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.localMatrix = new SkiaSharp.SKMatrix();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(36);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.localMatrix.marshal(pData + 0);
		}
	}
}
