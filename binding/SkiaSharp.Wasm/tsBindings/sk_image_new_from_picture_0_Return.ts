/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_from_picture_0_Return
	{
		/* Pack=4 */
		dimensions : SkiaSharp.SKSizeI;
		matrix : SkiaSharp.SKMatrix;
		public constructor()
		{
			this.dimensions = new SkiaSharp.SKSizeI();
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
			this.dimensions.marshal(pData + 0);
			this.matrix.marshal(pData + 8);
		}
	}
}
