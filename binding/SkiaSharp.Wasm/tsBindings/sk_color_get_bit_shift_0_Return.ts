/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_color_get_bit_shift_0_Return
	{
		/* Pack=4 */
		a : number;
		r : number;
		g : number;
		b : number;
		public constructor()
		{
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
			memoryContext.setValue(pData + 0, this.a, "i32");
			memoryContext.setValue(pData + 4, this.r, "i32");
			memoryContext.setValue(pData + 8, this.g, "i32");
			memoryContext.setValue(pData + 12, this.b, "i32");
		}
	}
}
