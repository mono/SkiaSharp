/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_get_pos_tan_0_Return
	{
		/* Pack=4 */
		position : SkiaSharp.SKPoint;
		tangent : SkiaSharp.SKPoint;
		public constructor()
		{
			this.position = new SkiaSharp.SKPoint();
			this.tangent = new SkiaSharp.SKPoint();
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
			this.position.marshal(pData + 0);
			this.tangent.marshal(pData + 8);
		}
	}
}
