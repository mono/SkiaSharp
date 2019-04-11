/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_get_point_0_Return
	{
		/* Pack=4 */
		point : SkiaSharp.SKPoint;
		public constructor()
		{
			this.point = new SkiaSharp.SKPoint();
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
			this.point.marshal(pData + 0);
		}
	}
}
