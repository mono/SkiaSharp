/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_is_rect_0_Return
	{
		/* Pack=4 */
		rect : SkiaSharp.SKRect;
		isClosed : boolean;
		direction : number;
		public constructor()
		{
			this.rect = new SkiaSharp.SKRect();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(24);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.rect.marshal(pData + 0);
			memoryContext.setValue(pData + 16, this.isClosed, "i32");
			memoryContext.setValue(pData + 20, this.direction, "i32");
		}
	}
}
